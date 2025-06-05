using Microsoft.EntityFrameworkCore;
using ToAquiBrasil.Core.Models;
using ToAquiBrasil.Core.Queries.Abstractions;
using ToAquiBrasil.Data;
using NetTopologySuite.Geometries;
using ToAquiBrasil.Core.Services;
using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Core.Queries;

public class LayoutQueries(ToAquiBrasilDbContext context, IRadiusConverterService radiusConverterService,
    IPointFabric pointFabric) : ILayoutQueries
{
  
    private readonly ToAquiBrasilDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IRadiusConverterService _radiusConverterService = radiusConverterService ?? throw new ArgumentNullException(nameof(radiusConverterService));
    private readonly IPointFabric _pointFabric = pointFabric ?? throw new ArgumentNullException(nameof(pointFabric));

    public async Task<LocationBasedLayoutModel> GetLocationBasedLayoutAsync(double latitude, double longitude, int radius, string unit, CancellationToken cancellationToken)
    {
        // Create a point using the provided coordinates
        var userLocation = _pointFabric.Create(latitude, longitude);
        
        var radiusInMeters = _radiusConverterService.ConvertRadiusToMeters(radius, unit);
        
        // Query listings within the radius in a single database operation
        var nearbyListings = await _context.Listings
            .Where(l => l.Location.Distance(userLocation) <= radiusInMeters)
            .ToListAsync(cancellationToken);
            
        // Extract categories and tags in memory to avoid additional database queries
        var categories = nearbyListings
            .Select(l => l.Category)
            .Distinct()
            .Select(c => new CategoryModel(c))
            .ToArray();
            
        var tags = nearbyListings
            .SelectMany(l => l.Tags)
            .Where(tag => !string.IsNullOrEmpty(tag))
            .Distinct()
            .Select(tag => new TagModel(tag))
            .ToArray();

        var radii = await GetRadiiAsync(unit);
            
        return new LocationBasedLayoutModel(
            new CategoriesModel(categories),
            new TagsModel(tags),
            new RadiiModel(radii)
        );
    }

    private static Task<RadiusModel[]> GetRadiiAsync(string unit = "km")
    {
        RadiusModel[] radii = unit.ToLowerInvariant() switch
        {
            "mi" => [
                new RadiusModel("1 MI", "1"),
                new RadiusModel("3 MI", "3"),
                new RadiusModel("5 MI", "5"),
                new RadiusModel("10 MI", "10")
            ],
            _ => [
                new RadiusModel("2 KM", "2"),
                new RadiusModel("5 KM", "5"),
                new RadiusModel("10 KM", "10"),
                new RadiusModel("15 KM", "15")
            ]
        };
        
        return Task.FromResult(radii);
    }
}