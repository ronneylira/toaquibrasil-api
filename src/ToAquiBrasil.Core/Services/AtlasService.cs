using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ToAquiBrasil.Core.Models;
using ToAquiBrasil.Core.Services.Abstractions;
using ToAquiBrasil.Core.Services.Dtos;

namespace ToAquiBrasil.Core.Services;

public class AtlasService : IGeoReverseService
{
    private readonly HttpClient _httpClient;
    private readonly IAtlasApiConfig _config;
    private readonly ILogger<AtlasService> _logger;

    public AtlasService(HttpClient httpClient, IAtlasApiConfig config, ILogger<AtlasService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CityModel> GetGeoReverseAsync(double latitude, double longitude,
        CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri($"reverseGeocode?api-version=2023-06-01&coordinates={longitude},{latitude}&subscription-key={_config.ApiKey}", UriKind.Relative);
            _logger.LogInformation("Calling Atlas API with coordinates: {Latitude}, {Longitude}", latitude, longitude);
            
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Atlas API request failed with status code {StatusCode}. Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<AtlasGeoReverseResponseDto>();
            if (result?.features == null || !result.features.Any())
            {
                _logger.LogWarning("Atlas API returned no features for coordinates: {Latitude}, {Longitude}", 
                    latitude, longitude);
                return null;
            }

            var cityName = result.features[0]?.properties?.address?.locality;
            if (string.IsNullOrEmpty(cityName))
            {
                _logger.LogWarning("Atlas API returned no locality name for coordinates: {Latitude}, {Longitude}", 
                    latitude, longitude);
                return null;
            }

            return new CityModel { Name = cityName };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Atlas API for coordinates: {Latitude}, {Longitude}", 
                latitude, longitude);
            return null;
        }
    }
}