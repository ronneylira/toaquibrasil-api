using Riok.Mapperly.Abstractions;
using ToAquiBrasil.Api.Dtos;
using ToAquiBrasil.Core.Models.Domain;

namespace ToAquiBrasil.Api.Mappers;

[Mapper]
public partial class ListingMapper
{
    /// <summary>
    /// Map domain results to GeoJSON DTO format
    /// </summary>
    public GeoJsonDto MapToGeoJsonDto(ListingResults results)
    {
        var features = results.Items.Select(MapToFeatureDto).ToArray();
        return new GeoJsonDto 
        { 
            Features = features,
            Metadata = new GeoJsonMetadata
            {
                Count = features.Length,
                GeneratedAt = DateTime.UtcNow,
                Search = results.SearchInfo != null ? new SearchInfo
                {
                    CenterLatitude = results.SearchInfo.CenterLatitude,
                    CenterLongitude = results.SearchInfo.CenterLongitude,
                    RadiusInMeters = results.SearchInfo.RadiusInMeters,
                    Unit = results.SearchInfo.Unit
                } : null
            }
        };
    }

    /// <summary>
    /// Map single domain result to GeoJSON DTO format
    /// </summary>
    public GeoJsonDto MapToGeoJsonDto(ListingResult? result)
    {
        if (result == null)
            return new GeoJsonDto 
            { 
                Features = [],
                Metadata = new GeoJsonMetadata { Count = 0 }
            };

        var results = new ListingResults 
        { 
            Items = [result], 
            TotalCount = 1 
        };
        
        return MapToGeoJsonDto(results);
    }

    /// <summary>
    /// Map domain result to detailed listing DTO
    /// </summary>
    [MapProperty(nameof(ListingResult.ExternalId), nameof(ListingDetailDto.Id))]
    [MapProperty(nameof(ListingResult.Latitude), nameof(ListingDetailDto.Latitude))]
    [MapProperty(nameof(ListingResult.Longitude), nameof(ListingDetailDto.Longitude))]
    public partial ListingDetailDto MapToListingDetailDto(ListingResult listing);

    /// <summary>
    /// Map listing image result to detail DTO
    /// </summary>
    public partial ListingImageDetailDto MapToImageDetailDto(ListingImageResult image);

    /// <summary>
    /// Map review result to detail DTO
    /// </summary>
    [MapProperty(nameof(ReviewResult.ExternalId), nameof(ReviewDetailDto.Id))]
    public partial ReviewDetailDto MapToReviewDetailDto(ReviewResult review);

    /// <summary>
    /// Map opening hours result to detail DTO
    /// </summary>
    public partial OpeningHoursDetailDto MapToOpeningHoursDetailDto(OpeningHoursResult openingHours);

    /// <summary>
    /// Map contact result to detail DTO
    /// </summary>
    public partial ContactDetailDto MapToContactDetailDto(ContactResult contact);

    /// <summary>
    /// Map domain model to Feature DTO
    /// </summary>
    private FeaturesDto MapToFeatureDto(ListingResult listing)
    {
        return new FeaturesDto
        {
            Type = "Feature",
            Geometry = MapToGeometryDto(listing),
            Properties = MapToPropertiesDto(listing)
        };
    }

    /// <summary>
    /// Map domain model to Geometry DTO
    /// </summary>
    private GeometryDto MapToGeometryDto(ListingResult listing)
    {
        return new GeometryDto
        {
            Type = "Point",
            Longitude = listing.Longitude,
            Latitude = listing.Latitude
        };
    }

    /// <summary>
    /// Map domain model to Properties DTO
    /// </summary>
    [MapProperty(nameof(ListingResult.ExternalId), nameof(PropertiesDto.Id))]
    private partial PropertiesDto MapToPropertiesDto(ListingResult listing);
}