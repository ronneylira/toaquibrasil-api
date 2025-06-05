using Riok.Mapperly.Abstractions;
using ToAquiBrasil.Core.Models.Domain;
using ToAquiBrasil.Core.Services.Abstractions;
using ToAquiBrasil.Core.Utils;
using ToAquiBrasil.Data.Entities;

namespace ToAquiBrasil.Core.Mappers;

[Mapper]
public partial class ListingEntityMapper
{
    private readonly IOpeningHoursService _openingHoursService;

    public ListingEntityMapper(IOpeningHoursService openingHoursService)
    {
        _openingHoursService = openingHoursService ?? throw new ArgumentNullException(nameof(openingHoursService));
    }

    /// <summary>
    /// Map database entity to domain model
    /// </summary>
    public ListingResult MapToDomain(Listing listing)
    {
        // Extract coordinate values
        var locationX = listing.Location?.X ?? 0;
        var locationY = listing.Location?.Y ?? 0;
        
        // Map opening hours first
        var openingHours = listing.OpeningHours?.Select(MapToOpeningHoursResult).ToList() ?? [];
        
        // Calculate overall opening status using the service
        var (isOpen, openingStatus) = _openingHoursService.CalculateOverallOpeningStatus(openingHours);
        
        // Create the result with explicit coordinate mapping
        var result = new ListingResult
        {
            ExternalId = listing.ExternalId,
            Index = listing.Index,
            IsActive = true, // Default to active for existing listings in database
            Logo = listing.Logo ?? string.Empty,
            Image = $"{listing.ExternalId}/{listing.Image ?? string.Empty}",
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
            Tags = listing.Tags ?? [],
            Services = listing.Services ?? [],
            
            // Explicit coordinate mapping
            Latitude = locationY,
            Longitude = locationX,
            
            DistanceInMeters = null,
            
            // Overall opening status
            IsOpen = isOpen,
            OpeningStatus = openingStatus,
            
            // Map collections using custom methods
            Reviews = listing.Reviews?.Select(MapToReviewResult).ToList() ?? [],
            OpeningHours = openingHours,
            Contacts = listing.Contacts?.Select(MapToContactResult).ToList() ?? [],
            Gallery = listing.Images?.Select(image => MapToListingImageResult(listing.ExternalId, image)).ToList() ?? []
        };
        
        return result;
    }

    /// <summary>
    /// Map OpeningHours entity to domain model
    /// </summary>
    public OpeningHoursResult MapToOpeningHoursResult(Data.Entities.ValueObjects.OpeningHours openingHours)
    {
        return new OpeningHoursResult
        {
            Day = _openingHoursService.GetDayName(openingHours.DayOfWeek),
            Hours = openingHours.Hours ?? string.Empty
        };
    }

    /// <summary>
    /// Map Review entity to domain model with field name mapping
    /// </summary>
    public ReviewResult MapToReviewResult(Data.Entities.ValueObjects.Review review)
    {
        return new ReviewResult
        {
            ExternalId = Guid.NewGuid(), // Generate new GUID for each review
            AuthorName = review.Title ?? string.Empty,
            Avatar = string.Empty, // Empty for now
            Stars = review.Stars,
            Content = review.Content ?? string.Empty,
            Date = DateFormatHelper.FormatReviewDate(review.Date),
            CreatedAt = review.Date
        };
    }

    /// <summary>
    /// Map Contact entity to domain model with field name mapping
    /// </summary>
    public ContactResult MapToContactResult(Data.Entities.ValueObjects.Contact contact)
    {
        return new ContactResult
        {
            Type = contact.Icon ?? string.Empty, // Use Icon as Type for now
            Icon = contact.Icon ?? string.Empty,
            Content = contact.Content ?? string.Empty,
            Link = contact.Link ?? string.Empty
        };
    }

    /// <summary>
    /// Map ListingImage entity to domain model with field name mapping
    /// </summary>
    public ListingImageResult MapToListingImageResult(Guid id,Data.Entities.ValueObjects.ListingImage image)
    {
        return new ListingImageResult
        {
            Url = $"{id}/{image.Image ?? string.Empty}",
            Alt = image.Title ?? string.Empty,
            Caption = image.Title ?? string.Empty
        };
    }
} 