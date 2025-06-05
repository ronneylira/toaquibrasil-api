namespace ToAquiBrasil.Api.Dtos;

/// <summary>
/// Detailed listing information for single listing responses
/// </summary>
public record ListingDetailDto
{
    public Guid Id { get; init; }
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
    
    // Location information
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double? DistanceInMeters { get; init; }
    
    // Opening status (calculated from opening hours)
    public bool IsOpen { get; init; }
    public string OpeningStatus { get; init; } = string.Empty; // "Open now", "Opening in 15 minutes", "Closed"
    
    // Related data
    public ReviewDetailDto[] Reviews { get; init; } = [];
    public OpeningHoursDetailDto[] OpeningHours { get; init; } = [];
    public ContactDetailDto[] Contacts { get; init; } = [];
    
    // Computed properties
    public int ReviewCount => Reviews.Length;
    public string Description => Summary;
    public ListingImageDetailDto[] Gallery { get; init; } = [];
    public string[] Amenities => Services;
}

/// <summary>
/// Image information for listing details
/// </summary>
public record ListingImageDetailDto
{
    public string Url { get; init; } = string.Empty;
    public string Alt { get; init; } = string.Empty;
    public string Caption { get; init; } = string.Empty;
}

/// <summary>
/// Review information for listing details
/// </summary>
public record ReviewDetailDto
{
    public Guid Id { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty; // Empty for now
    public int Stars { get; init; }
    public string Content { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty; // Formatted as "Dec 2018"
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Opening hours information for listing details
/// </summary>
public record OpeningHoursDetailDto
{
    public string Day { get; init; } = string.Empty;
    public string Hours { get; init; } = string.Empty;
}

/// <summary>
/// Contact information for listing details
/// </summary>
public record ContactDetailDto
{
    public string Type { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Link { get; init; } = string.Empty;
} 