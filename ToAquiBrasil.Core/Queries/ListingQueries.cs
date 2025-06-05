using Microsoft.EntityFrameworkCore;
using ToAquiBrasil.Core.Mappers;
using ToAquiBrasil.Core.Models.Domain;
using ToAquiBrasil.Core.Queries.Abstractions;
using ToAquiBrasil.Core.Services.Abstractions;
using ToAquiBrasil.Core.Utils;
using ToAquiBrasil.Data;

namespace ToAquiBrasil.Core.Queries;

public class ListingQueries(
    ToAquiBrasilDbContext dbContext,
    IRadiusConverterService radiusConverterService,
    IPointFabric pointFabric,
    ListingEntityMapper entityMapper,
    IOpeningHoursService openingHoursService) : IListingQueries
{
    private readonly ToAquiBrasilDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IRadiusConverterService _radiusConverterService =
        radiusConverterService ?? throw new ArgumentNullException(nameof(radiusConverterService));
    private readonly IPointFabric _pointFabric = pointFabric ?? throw new ArgumentNullException(nameof(pointFabric));
    private readonly ListingEntityMapper _entityMapper = entityMapper ?? throw new ArgumentNullException(nameof(entityMapper));
    private readonly IOpeningHoursService _openingHoursService = openingHoursService ?? throw new ArgumentNullException(nameof(openingHoursService));

    public async Task<ListingResults> GetListingResultsAsync(
        double latitude,
        double longitude,
        int radius,
        string unit,
        GetListingsFilterModel? filterModel = null,
        CancellationToken cancellationToken = default)
    {
        var userLocation = _pointFabric.Create(latitude, longitude);
        var radiusInMeters = _radiusConverterService.ConvertRadiusToMeters(radius, unit);

        // Start with base query for listings within radius
        var query = _dbContext.Listings
            .AsNoTracking()
            .Where(listing => listing.Location.Distance(userLocation) <= radiusInMeters);

        // Apply filters
        query = ApplyFilters(query, filterModel);

        // Execute query with projection to fetch only needed fields
        var retrievedListings = await query
            .Select(listing => new 
            {
                listing.ExternalId,
                listing.Index,
                listing.Logo,
                listing.Image,
                listing.Link,
                listing.Name,
                listing.Category,
                listing.Person,
                listing.Email,
                listing.Stars,
                listing.Phone,
                listing.Address,
                listing.About,
                listing.Summary,
                listing.Tags,
                listing.Services,
                Latitude = listing.Location.Y,
                Longitude = listing.Location.X,
                DistanceInMeters = listing.Location.Distance(userLocation),
                Images = listing.Images.Select(img => new { img.Image, img.Title }).ToList(),
                Reviews = listing.Reviews.Select(r => new { r.Title, r.Stars, r.Content, r.Date }).ToList(),
                OpeningHours = listing.OpeningHours.Select(oh => new { oh.DayOfWeek, oh.Hours }).ToList(),
                Contacts = listing.Contacts.Select(c => new { c.Icon, c.Content, c.Link }).ToList()
            })
            .ToListAsync(cancellationToken);

        // Apply in-memory tag filtering if needed
        var filteredListings = ApplyInMemoryFiltering(retrievedListings, filterModel);

        // Convert to domain models (manual mapping since we have projected data)
        var listingResults = filteredListings.Select(MapProjectedDataToDomain).ToList();

        return new ListingResults
        {
            Items = listingResults,
            TotalCount = listingResults.Count,
            SearchInfo = new SearchMetadata
            {
                CenterLatitude = latitude,
                CenterLongitude = longitude,
                RadiusInMeters = radiusInMeters,
                Unit = unit,
                ResultCount = listingResults.Count
            }
        };
    }

    public async Task<ListingResult?> GetListingResultByExternalIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        var listing = await _dbContext.Listings
            .AsNoTracking()
            .Include(l => l.Images)
            .Include(l => l.Reviews)
            .Include(l => l.OpeningHours)
            .Include(l => l.Contacts)
            .Where(listing => listing.ExternalId == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (listing == null)
            return null;

        // Convert to domain model using Mapperly
        return _entityMapper.MapToDomain(listing);
    }

    private static IQueryable<Data.Entities.Listing> ApplyFilters(
        IQueryable<Data.Entities.Listing> query, 
        GetListingsFilterModel? filterModel)
    {
        if (filterModel == null)
            return query;

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(filterModel.Category))
        {
            query = query.Where(listing => listing.Category == filterModel.Category);
        }

        // Apply keyword search
        if (!string.IsNullOrWhiteSpace(filterModel.Keyword))
        {
            query = query.Where(listing =>
                listing.Name.Contains(filterModel.Keyword) ||
                listing.About.Contains(filterModel.Keyword) ||
                listing.Address.Contains(filterModel.Keyword));
        }

        return query;
    }

    private static List<T> ApplyInMemoryFiltering<T>(List<T> listings, GetListingsFilterModel? filterModel)
        where T : class
    {
        if (filterModel?.Tags == null || !filterModel.Tags.Any())
            return listings;

        var tagArray = filterModel.Tags
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        if (tagArray.Length == 0)
            return listings;

        // Use reflection to get Tags property from anonymous type
        var tagsProperty = typeof(T).GetProperty("Tags");
        if (tagsProperty == null)
            return listings;

        return listings.Where(listing =>
        {
            var listingTags = tagsProperty.GetValue(listing) as string[];
            if (listingTags == null) return false;

            return tagArray.All(tagToFind =>
                listingTags.Any(listingTag =>
                    listingTag.Trim().Equals(tagToFind, StringComparison.OrdinalIgnoreCase)
                )
            );
        }).ToList();
    }

    /// <summary>
    /// Helper method to map projected data to domain model
    /// This is simplified manual mapping since we use projection for performance
    /// </summary>
    private ListingResult MapProjectedDataToDomain(dynamic listing)
    {
        // Map opening hours first
        var openingHours = ((IEnumerable<dynamic>)listing.OpeningHours).Select(oh => new OpeningHoursResult
        {
            Day = _openingHoursService.GetDayName(oh.DayOfWeek),
            Hours = oh.Hours ?? string.Empty
        }).ToList();
        
        // Calculate overall opening status using the service
        var (isOpen, openingStatus) = _openingHoursService.CalculateOverallOpeningStatus(openingHours);
        
        return new ListingResult
        {
            ExternalId = listing.ExternalId,
            Index = listing.Index,
            IsActive = true, // Default to active for existing listings
            Logo = listing.Logo ?? string.Empty,
            Image = listing.Image ?? string.Empty,
            Link = listing.Link ?? string.Empty,
            Name = listing.Name ?? string.Empty,
            Category = listing.Category ?? string.Empty,
            Person = listing.Person ?? string.Empty,
            Email = listing.Email ?? string.Empty,
            Stars = listing.Stars,
            Phone = listing.Phone ?? string.Empty,
            Address = listing.Address ?? string.Empty,
            About = listing.About ?? string.Empty,
            Summary = listing.Summary ?? string.Empty,
            Tags = listing.Tags ?? new string[0],
            Services = listing.Services ?? new string[0],
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            DistanceInMeters = listing.DistanceInMeters,
            
            // Overall opening status
            IsOpen = isOpen,
            OpeningStatus = openingStatus,
            
            Gallery = ((IEnumerable<dynamic>)listing.Images).Select(img => new ListingImageResult
            {
                Url = img.Image ?? string.Empty,
                Alt = img.Title ?? string.Empty,
                Caption = img.Title ?? string.Empty
            }).ToList(),
            Reviews = ((IEnumerable<dynamic>)listing.Reviews).Select(review => new ReviewResult
            {
                ExternalId = Guid.NewGuid(),
                AuthorName = review.Title ?? string.Empty,
                Avatar = string.Empty, // Empty for now
                Stars = review.Stars,
                Content = review.Content ?? string.Empty,
                Date = DateFormatHelper.FormatReviewDate(review.Date),
                CreatedAt = review.Date
            }).ToList(),
            OpeningHours = openingHours,
            Contacts = ((IEnumerable<dynamic>)listing.Contacts).Select(contact => new ContactResult
            {
                Type = contact.Icon ?? string.Empty,
                Icon = contact.Icon ?? string.Empty,
                Content = contact.Content ?? string.Empty,
                Link = contact.Link ?? string.Empty
            }).ToList()
        };
    }
}
