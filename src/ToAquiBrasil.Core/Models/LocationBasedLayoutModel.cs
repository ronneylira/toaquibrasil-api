namespace ToAquiBrasil.Core.Models;

/// <summary>
/// Aggregates layout-related data for location-based queries
/// </summary>
public class LocationBasedLayoutModel(CategoriesModel categories, TagsModel tags, RadiiModel radii)
{
    public CategoriesModel Categories { get; } = categories;
    
    public TagsModel Tags { get; } = tags;

    public RadiiModel Radii { get; } = radii;
} 