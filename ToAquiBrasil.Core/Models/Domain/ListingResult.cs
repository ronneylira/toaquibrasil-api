namespace ToAquiBrasil.Core.Models.Domain;

/// <summary>
/// Generic domain model representing a listing result
/// This is format-agnostic and focused on business data
/// </summary>
public record ListingResult
{
    public Guid ExternalId { get; init; }
    public int Index { get; init; }
    public bool IsActive { get; init; }
    public string Logo { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public string Link { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Person { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Stars { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string About { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string[] Tags { get; init; } = [];
    public string[] Services { get; init; } = [];
    
    // Geographic data as simple properties
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    
    // Distance from search point (calculated during query)
    public double? DistanceInMeters { get; init; }
    
    // Opening status (calculated from opening hours)
    public bool IsOpen { get; init; }
    public string OpeningStatus { get; init; } = string.Empty; // "Open now", "Opening in 15 minutes", "Closed"
    
    // Related data collections
    public IReadOnlyList<ReviewResult> Reviews { get; init; } = [];
    public IReadOnlyList<OpeningHoursResult> OpeningHours { get; init; } = [];
    public IReadOnlyList<ContactResult> Contacts { get; init; } = [];
    
    // Computed properties for frontend compatibility
    public int ReviewCount => Reviews.Count;
    public string Description => Summary;
    public IReadOnlyList<ListingImageResult> Gallery { get; init; } = [];
    public string[] Amenities => Services;
}

/// <summary>
/// Domain model for listing images
/// </summary>
public class ListingImageResult
{
    public string Url { get; init; } = string.Empty;
    public string Alt { get; init; } = string.Empty;
    public string Caption { get; init; } = string.Empty;
}

/// <summary>
/// Domain model for reviews
/// </summary>
public class ReviewResult
{
    public Guid ExternalId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty; // Empty for now
    public int Stars { get; init; }
    public string Content { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty; // Formatted as "Dec 2018"
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Domain model for opening hours
/// </summary>
public class OpeningHoursResult
{
    public string Day { get; init; } = string.Empty;
    public string Hours { get; init; } = string.Empty;
}

/// <summary>
/// Domain model for contacts
/// </summary>
public class ContactResult
{
    public string Type { get; init; } = string.Empty; // email, phone, website, facebook, etc.
    public string Icon { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Link { get; init; } = string.Empty;
}

/// <summary>
/// Collection of listing results with metadata
/// </summary>
public class ListingResults
{
    public IReadOnlyList<ListingResult> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public SearchMetadata SearchInfo { get; init; } = new();
}

/// <summary>
/// Information about the search that was performed
/// </summary>
public class SearchMetadata
{
    public double CenterLatitude { get; init; }
    public double CenterLongitude { get; init; }
    public double RadiusInMeters { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime SearchTime { get; init; } = DateTime.UtcNow;
    public int ResultCount { get; init; }
} 