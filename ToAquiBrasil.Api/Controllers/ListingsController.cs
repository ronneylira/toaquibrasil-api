using Microsoft.AspNetCore.Mvc;
using ToAquiBrasil.Api.Dtos;
using ToAquiBrasil.Core.Models.Domain;
using ToAquiBrasil.Core.Queries.Abstractions;
using ToAquiBrasil.Api.Mappers;

namespace ToAquiBrasil.Api.Controllers;

/// <summary>
/// API controller for listing operations with flexible transformation to different presentation formats
/// </summary>
[ApiController]
[Route("[controller]")]
public class ListingsController(IListingQueries listingQueries, ListingMapper mapper) : ApiControllerBase
{
    private readonly IListingQueries _listingQueries = listingQueries ?? throw new ArgumentNullException(nameof(listingQueries));
    private readonly ListingMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <summary>
    /// Get listings in GeoJSON format (for mapping applications)
    /// </summary>
    [HttpGet("geojson")]
    public async Task<ActionResult<ApiResponse<GeoJsonDto>>> GetListingsGeoJson(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] int radius = 1,
        [FromQuery] string unit = "km",
        [FromQuery] string? category = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        var filterModel = new GetListingsFilterModel
        {
            Category = category,
            Keyword = keyword,
            Tags = tags?.ToList()
        };

        var results = await _listingQueries.GetListingResultsAsync(lat, lng, radius, unit, filterModel, cancellationToken);
        var geoJsonDto = _mapper.MapToGeoJsonDto(results);
        
        return Success(geoJsonDto);
    }
    
    /// <summary>
    /// Get single listing by ID in detailed format
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ListingDetailDto>>> GetListing(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _listingQueries.GetListingResultByExternalIdAsync(id, cancellationToken);
        
        if (result == null)
            return Error<ListingDetailDto>($"Listing with ID {id} not found", 404);

        var listingDetailDto = _mapper.MapToListingDetailDto(result);
        return Success(listingDetailDto);
    }

    /// <summary>
    /// Legacy endpoint for existing frontend compatibility
    /// GET /Listings/lat/{lat}/long/{lng}/radius/{radius}/{unit}
    /// </summary>
    [HttpGet("lat/{latitude}/long/{longitude}/radius/{radius}/{unit}")]
    public async Task<ActionResult<ApiResponse<GeoJsonDto>>> GetListingsLegacyFormat(
        double latitude,
        double longitude,
        int radius,
        string unit,
        [FromQuery] string? keyword = null,
        [FromQuery] string? tags = null,
        CancellationToken cancellationToken = default)
    {
        var tagsList = string.IsNullOrWhiteSpace(tags)
            ? null
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList();

        var filterModel = new GetListingsFilterModel
        {
            Category = null,
            Keyword = keyword,
            Tags = tagsList
        };

        var results = await _listingQueries.GetListingResultsAsync(latitude, longitude, radius, unit, filterModel, cancellationToken);
        var geoJsonDto = _mapper.MapToGeoJsonDto(results);
        
        return Success(geoJsonDto);
    }

    /// <summary>
    /// Legacy endpoint with category filter
    /// GET /Listings/lat/{lat}/long/{lng}/radius/{radius}/{unit}/category/{category}
    /// </summary>
    [HttpGet("lat/{latitude}/long/{longitude}/radius/{radius}/{unit}/category/{category}")]
    public async Task<ActionResult<ApiResponse<GeoJsonDto>>> GetListingsByCategoryLegacyFormat(
        double latitude,
        double longitude,
        int radius,
        string unit,
        string category,
        [FromQuery] string? keyword = null,
        [FromQuery] string? tags = null,
        CancellationToken cancellationToken = default)
    {
        var tagsList = string.IsNullOrWhiteSpace(tags)
            ? null
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList();

        var filterModel = new GetListingsFilterModel
        {
            Category = category,
            Keyword = keyword,
            Tags = tagsList
        };

        var results = await _listingQueries.GetListingResultsAsync(latitude, longitude, radius, unit, filterModel, cancellationToken);
        var geoJsonDto = _mapper.MapToGeoJsonDto(results);
        
        return Success(geoJsonDto);
    }
}
