using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ToAquiBrasil.Core.Models;
using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Core.Services;

public class OpenStreetMapService : IGeoReverseService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenStreetMapService> _logger;
    private const string UserAgent = "ToAquiBrasil/1.0"; // Required by Nominatim's terms of use

    public OpenStreetMapService(HttpClient httpClient, ILogger<OpenStreetMapService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public async Task<CityModel> GetGeoReverseAsync(double latitude, double longitude,
        CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri($"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&addressdetails=1", UriKind.Absolute);
            _logger.LogInformation("Calling OpenStreetMap API with coordinates: {Latitude}, {Longitude}", latitude, longitude);
            
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenStreetMap API request failed with status code {StatusCode}. Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<OpenStreetMapResponseDto>(cancellationToken);
            if (result?.address == null)
            {
                _logger.LogWarning("OpenStreetMap API returned no address for coordinates: {Latitude}, {Longitude}", 
                    latitude, longitude);
                return null;
            }

            // Try to get the city name from different fields
            var cityName = result.address.city 
                ?? result.address.town 
                ?? result.address.village 
                ?? result.address.municipality;

            if (string.IsNullOrEmpty(cityName))
            {
                _logger.LogWarning("OpenStreetMap API returned no city name for coordinates: {Latitude}, {Longitude}", 
                    latitude, longitude);
                return null;
            }

            return new CityModel { Name = cityName };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenStreetMap API for coordinates: {Latitude}, {Longitude}", 
                latitude, longitude);
            return null;
        }
    }
}

public class OpenStreetMapResponseDto
{
    public Address address { get; init; }
}

public class Address
{
    public string city { get; set; }
    public string town { get; set; }
    public string village { get; set; }
    public string municipality { get; set; }
} 