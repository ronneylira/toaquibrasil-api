namespace ToAquiBrasil.Api.Dtos;

/// <summary>
/// Represents a GeoJSON FeatureCollection
/// Follows RFC 7946 specification for GeoJSON
/// </summary>
public record GeoJsonDto
{
    /// <summary>
    /// Always "FeatureCollection" for GeoJSON FeatureCollection
    /// </summary>
    public static string Type => "FeatureCollection";
    
    /// <summary>
    /// Array of GeoJSON features
    /// </summary>
    public FeaturesDto[] Features { get; init; } = [];
    
    /// <summary>
    /// Metadata about the feature collection
    /// </summary>
    public GeoJsonMetadata? Metadata { get; init; }
}

/// <summary>
/// Metadata for the GeoJSON FeatureCollection
/// </summary>
public record GeoJsonMetadata
{
    /// <summary>
    /// Total number of features in the collection
    /// </summary>
    public int Count { get; init; }
    
    /// <summary>
    /// Timestamp when the data was generated
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Search parameters used to generate this collection
    /// </summary>
    public SearchInfo? Search { get; init; }
}

/// <summary>
/// Information about the search that generated this GeoJSON
/// </summary>
public record SearchInfo
{
    /// <summary>
    /// Center latitude of the search
    /// </summary>
    public double CenterLatitude { get; init; }
    
    /// <summary>
    /// Center longitude of the search
    /// </summary>
    public double CenterLongitude { get; init; }
    
    /// <summary>
    /// Search radius in meters
    /// </summary>
    public double RadiusInMeters { get; init; }
    
    /// <summary>
    /// Original unit used for the search
    /// </summary>
    public string Unit { get; init; } = string.Empty;
} 