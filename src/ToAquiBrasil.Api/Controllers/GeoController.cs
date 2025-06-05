using Microsoft.AspNetCore.Mvc;
using ToAquiBrasil.Api.Dtos;
using ToAquiBrasil.Api.Mappers;
using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class GeoController(
    ISearchCityService searchCityService,
    IIpLocationService ipLocationService,
    IGeoReverseService atlasService,
    IGeocodingService geocodingService,
    CitiesMapper citiesMapper,
    CityMapper cityMapper,
    LocationMapper locationMapper)
    : ApiControllerBase
{
    private readonly ISearchCityService _searchCityService =
        searchCityService ?? throw new ArgumentNullException(nameof(searchCityService));

    private readonly IIpLocationService _ipLocationService =
        ipLocationService ?? throw new ArgumentNullException(nameof(ipLocationService));

    private readonly IGeoReverseService _atlasService =
        atlasService ?? throw new ArgumentNullException(nameof(atlasService));

    private readonly IGeocodingService _geocodingService =
        geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));

    private readonly CitiesMapper _citiesMapper = citiesMapper ?? throw new ArgumentNullException(nameof(citiesMapper));

    private readonly CityMapper _cityMapper = cityMapper ?? throw new ArgumentNullException(nameof(cityMapper));

    private readonly LocationMapper _locationMapper =
        locationMapper ?? throw new ArgumentNullException(nameof(locationMapper));

    [HttpGet("city/{cityName}")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["cityName", "country",])]
    public async Task<ActionResult<ApiResponse<CitiesDto>>> SearchCityByNameInCountry(string cityName, string country,
        CancellationToken cancellationToken = default)
    {
        var cities = await _searchCityService.SearchCityAsync(cityName, country, cancellationToken);
        var citiesDto = _citiesMapper.MapToCitiesDto(cities);
        return Success(citiesDto);
    }

    [HttpGet("ip/{ip}")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["ip",])]
    public async Task<ActionResult<ApiResponse<IpLocationDto>>> GetIpLocation(string? ip,
        CancellationToken cancellationToken = default)
    {
        // Get basic location info from IP service to get city name
        var ipLocation = await _ipLocationService.GetIpInfoAsync(ip, cancellationToken);
        
        try
        {
            // Use geocoding service to get precise coordinates for the city
            var cityQuery = $"{ipLocation.City}, {ipLocation.Country}";
            var locationFromGeocoding = await _geocodingService.GetCoordinatesByCityAsync(cityQuery, cancellationToken);
            
            return Success(new IpLocationDto
            {
                Country = ipLocation.Country,
                City = ipLocation.City,
                Latitude = locationFromGeocoding.Latitude,
                Longitude = locationFromGeocoding.Longitude
            });
        }
        catch (KeyNotFoundException)
        {
            // Fallback to original IP location coordinates if geocoding fails
            return Success(new IpLocationDto
            {
                Country = ipLocation.Country,
                City = ipLocation.City,
                Latitude = ipLocation.Latitude,
                Longitude = ipLocation.Longitude
            });
        }
    }

    [HttpGet("city/lat/{latitude}/long/{longitude}")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["latitude", "longitude",])]
    public async Task<ActionResult<ApiResponse<CityDto>>> GetCityByLatLongAsync(double latitude, double longitude,
        CancellationToken cancellationToken = default)
    {
        var city = await _atlasService.GetGeoReverseAsync(latitude, longitude, cancellationToken);
        var cityDto = _cityMapper.MapToCityDto(city);
        return Success(cityDto);
    }

    /// <summary>
    /// Get coordinates for a city name
    /// </summary>
    /// <param name="cityName">Name of the city</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Coordinates of the city</returns>
    [HttpGet("coordinates/city/{cityName}")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["cityName",])]
    public async Task<ActionResult<ApiResponse<LocationDto>>> GetCoordinatesByCityAsync(string cityName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var location = await _geocodingService.GetCoordinatesByCityAsync(cityName, cancellationToken);
            var locationDto = _locationMapper.MapToLocationDto(location);
            return Success(locationDto);
        }
        catch (KeyNotFoundException ex)
        {
            return Error<LocationDto>(ex.Message, 404);
        }
    }

    /// <summary>
    /// Suggest locations based on user input
    /// </summary>
    /// <param name="query">User input text</param>
    /// <param name="limit">Maximum number of suggestions (default: 5)</param>
    /// <param name="countryCode">Optional country code to filter results (e.g., 'us', 'br')</param>
    /// <param name="placeTypes">Optional comma-separated list of place types to filter (e.g., 'city,town')</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of location suggestions</returns>
    [HttpGet("suggest")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["query", "limit", "countryCode", "placeTypes"])]
    public async Task<ActionResult<ApiResponse<LocationSuggestionsDto>>> SuggestLocationsAsync(
        [FromQuery] string query,
        [FromQuery] int limit = 5,
        [FromQuery] string? countryCode = null,
        [FromQuery] string? placeTypes = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Success(new LocationSuggestionsDto {Query = query});
        }

        // Parse place types if provided
        List<string>? placeTypesList = null;
        if (!string.IsNullOrWhiteSpace(placeTypes))
        {
            placeTypesList = placeTypes.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        var suggestions = await _geocodingService.SuggestLocationsAsync(
            query,
            limit,
            countryCode,
            placeTypesList,
            cancellationToken);

        var suggestionsDto = _locationMapper.MapToLocationSuggestionsDto(suggestions, query);
        return Success(suggestionsDto);
    }

    /// <summary>
    /// Get location from the current user's IP address
    /// </summary>
    /// <returns>Location information based on the user's IP</returns>
    [HttpGet("ip-location")]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<ApiResponse<IpLocationDto>>> GetCurrentIpLocationAsync(
        CancellationToken cancellationToken = default)
    {
        // Try multiple approaches to get the real client IP

        var clientIp =
            // 1. Try standard remote IP address
            HttpContext.Connection.RemoteIpAddress?.ToString();

        // 2. Try common proxy headers in order of reliability
        if (string.IsNullOrWhiteSpace(clientIp) || clientIp == "127.0.0.1" || clientIp == "::1")
        {
            // Check X-Forwarded-For header (most common)
            clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
                ?.Trim();

            // Check other common proxy headers if still not found
            if (string.IsNullOrWhiteSpace(clientIp))
                clientIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(clientIp))
                clientIp = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault(); // Cloudflare

            if (string.IsNullOrWhiteSpace(clientIp))
                clientIp = HttpContext.Request.Headers["True-Client-IP"].FirstOrDefault(); // Akamai/Cloudflare

            if (string.IsNullOrWhiteSpace(clientIp))
                clientIp = HttpContext.Request.Headers["X-Client-IP"].FirstOrDefault();
        }

        // Get basic location info from IP service to get city name
        var ipLocationInfo = await _ipLocationService.GetIpInfoAsync(clientIp ?? string.Empty, cancellationToken);
        
        try
        {
            // Use geocoding service to get precise coordinates for the city
            var cityQuery = $"{ipLocationInfo.City}, {ipLocationInfo.Country}";
            var locationFromGeocoding = await _geocodingService.GetCoordinatesByCityAsync(cityQuery, cancellationToken);
            
            return Success(new IpLocationDto
            {
                Country = ipLocationInfo.Country,
                City = ipLocationInfo.City,
                Latitude = locationFromGeocoding.Latitude,
                Longitude = locationFromGeocoding.Longitude
            });
        }
        catch (KeyNotFoundException)
        {
            // Fallback to original IP location coordinates if geocoding fails
            return Success(new IpLocationDto
            {
                Country = ipLocationInfo.Country,
                City = ipLocationInfo.City,
                Latitude = ipLocationInfo.Latitude,
                Longitude = ipLocationInfo.Longitude
            });
        }
    }
}
