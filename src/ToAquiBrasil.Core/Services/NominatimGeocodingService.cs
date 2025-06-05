using System.Text.Json;
using Microsoft.Extensions.Logging;
using ToAquiBrasil.Core.Models;
using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Core.Services;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;

    // User agent is required by Nominatim's usage policy
    private const string UserAgent = "ToAquiBrasil/1.0";

    public NominatimGeocodingService(HttpClient httpClient, ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Set base URL and required headers
        _httpClient.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public async Task<LocationModel> GetCoordinatesByCityAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                throw new ArgumentException("City name is required", nameof(cityName));
            }

            _logger.LogInformation("Getting coordinates for city: {CityName}", cityName);

            // Build the search request
            var requestUri = $"search?q={Uri.EscapeDataString(cityName)}&format=json&addressdetails=1&limit=1";
            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Nominatim API request failed with status code {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to get coordinates. Status code: {response.StatusCode}");
            }

            // Parse the response
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
            var searchResults = JsonSerializer.Deserialize<List<NominatimSearchResult>>(content, jsonOptions);

            if (searchResults == null || searchResults.Count == 0)
            {
                _logger.LogWarning("No results found for city: {CityName}", cityName);
                throw new KeyNotFoundException($"No coordinates found for city: {cityName}");
            }

            var result = searchResults.First();
            return new LocationModel
            {
                Latitude = result.Lat,
                Longitude = result.Lon,
                DisplayName = FormatDisplayName(result.Address, result.DisplayName)
            };
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Error getting coordinates for city: {CityName}", cityName);
            throw;
        }
    }

    public async Task<List<LocationSuggestionModel>> SuggestLocationsAsync(string query, int limit = 5, string? countryCode = null, List<string>? placeTypes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<LocationSuggestionModel>();
            }

            _logger.LogInformation("Searching location suggestions for query: {Query}, countryCode: {CountryCode}, placeTypes: {PlaceTypes}",
                query, countryCode, placeTypes != null ? string.Join(",", placeTypes) : "null");

            // Focus search strictly on cities and populated places (no administrative boundaries)
            var requestUri = $"search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit={limit * 3}";

            // Add specific filters for place types - default to cities if not specified
            var featureTypes = "city,town,village";
            if (placeTypes is {Count: > 0})
            {
                // Use the supplied place types, ensuring they are valid
                var validPlaceTypes = placeTypes
                    .Where(IsValidPlaceType)
                    .ToList();

                if (validPlaceTypes.Count > 0)
                {
                    featureTypes = string.Join(",", validPlaceTypes);
                }
            }

            requestUri += $"&featureType={featureTypes}";

            // Force using OSM place class to get only populated places
            requestUri += "&class=place";

            // Add country filter if provided
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                requestUri += $"&countrycodes={countryCode.Trim().ToLower()}";
            }

            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Nominatim API request failed with status code {StatusCode}", response.StatusCode);
                return new List<LocationSuggestionModel>();
            }

            // Parse the response
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
            var searchResults = JsonSerializer.Deserialize<List<NominatimSearchResult>>(content, jsonOptions);

            if (searchResults == null || searchResults.Count == 0)
            {
                return new List<LocationSuggestionModel>();
            }

            // Post-process to strictly filter the results based on place types
            var allSuggestions = searchResults
                // Only include results with non-empty PlaceId
                .Where(r => r != null && r.Lat != 0 && r.Lon != 0)
                .Select(r => new LocationSuggestionModel
                {
                    Id = r.PlaceId ?? string.Empty,
                    DisplayName = FormatDisplayName(r.Address, r.DisplayName),
                    PlaceType = r.Type,
                    Latitude = r.Lat,
                    Longitude = r.Lon,
                    CountryCode = r.Address?.CountryCode ?? string.Empty,
                    AddressType = r.AddressType
                })
                // Apply filters based on place types and country code
                .Where(s =>
                    // Filter by place type - allow administrative types if they are cities
                    (placeTypes == null || placeTypes.Count == 0 ||
                    placeTypes.Contains(s.PlaceType, StringComparer.OrdinalIgnoreCase) ||
                    // Also check if addressType indicates a city-like place
                    (s.PlaceType.Equals("administrative", StringComparison.OrdinalIgnoreCase) &&
                     IsCityLikeAddressType(s.AddressType))) &&
                    // Filter by country code - but only if we have a non-empty country code to filter by
                    (string.IsNullOrWhiteSpace(countryCode) ||
                    string.Equals(s.CountryCode, countryCode, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(s => GetPlaceTypeRank(s.PlaceType))
                .ToList();

            // Remove duplicates by grouping by name and location
            var suggestions = allSuggestions
                // No ID filter, only filter out unknown locations
                .Where(s => s.DisplayName != "Unknown Location")
                // Group by name and approximate location (to detect duplicates)
                .GroupBy(s => new
                {
                    // Get the first part of the display name (the place name itself)
                    Name = s.DisplayName.Split(',').FirstOrDefault()?.Trim() ?? s.DisplayName,
                    // Round coordinates to remove small differences
                    Lat = Math.Round(s.Latitude, 3),
                    Lng = Math.Round(s.Longitude, 3)
                })
                // For each group, select the best representative
                .Select(g => SelectBestResult(g.ToList()))
                // Filter out null results from SelectBestResult
                .Where(s => s != null)
                .Take(limit)
                .ToList();

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location suggestions for query: {Query}", query);
            return new List<LocationSuggestionModel>();
        }
    }

    /// <summary>
    /// Checks if a place type is valid for use in the API
    /// </summary>
    private static bool IsValidPlaceType(string placeType)
    {
        if (string.IsNullOrWhiteSpace(placeType))
            return false;

        string normalized = placeType.Trim().ToLowerInvariant();

        // List of valid place types for population centers
        return normalized == "city" ||
               normalized == "town" ||
               normalized == "village" ||
               normalized == "municipality" ||
               normalized == "hamlet";
    }

    /// <summary>
    /// Determines if a place type is a city or populated place (no administrative boundaries)
    /// </summary>
    private bool IsCityOrPopulatedPlace(string placeType)
    {
        return placeType.Equals("city", StringComparison.OrdinalIgnoreCase) ||
               placeType.Equals("town", StringComparison.OrdinalIgnoreCase) ||
               placeType.Equals("village", StringComparison.OrdinalIgnoreCase) ||
               placeType.Equals("municipality", StringComparison.OrdinalIgnoreCase) ||
               placeType.Equals("hamlet", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a rank value for place types to sort by importance
    /// </summary>
    private static int GetPlaceTypeRank(string placeType)
    {
        return placeType.ToLowerInvariant() switch
        {
            "city" => 1,
            "municipality" => 2,
            "town" => 3,
            "village" => 4,
            "hamlet" => 5,
            _ => 99
        };
    }

    /// <summary>
    /// Creates a user-friendly display name from address components
    /// </summary>
    private static string FormatDisplayName(NominatimAddress? address, string fallbackName)
    {
        // Make sure we have a valid fallback name
        if (string.IsNullOrWhiteSpace(fallbackName))
        {
            fallbackName = "Unknown Location";
        }

        if (address == null)
        {
            return fallbackName;
        }

        // Get the most specific location name (city, town, village, etc.)
        string? locationName = address.City ?? address.Town ?? address.Village ??
                               address.Suburb ?? address.Neighbourhood ?? address.County;

        if (string.IsNullOrWhiteSpace(locationName))
        {
            return fallbackName;
        }

        // Format as "Location, State, Country"
        var parts = new List<string> {locationName,};

        if (!string.IsNullOrWhiteSpace(address.State))
        {
            parts.Add(address.State);
        }

        if (!string.IsNullOrWhiteSpace(address.Country))
        {
            parts.Add(address.Country);
        }

        string formattedName = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(formattedName) ? fallbackName : formattedName;
    }

    /// <summary>
    /// Checks if an address type represents a city-like place
    /// </summary>
    private static bool IsCityLikeAddressType(string addressType)
    {
        if (string.IsNullOrWhiteSpace(addressType))
            return false;

        string normalized = addressType.Trim().ToLowerInvariant();

        // List of address types that represent populated places
        return normalized == "city" ||
               normalized == "town" ||
               normalized == "village" ||
               normalized == "municipality" ||
               normalized == "hamlet";
    }

    /// <summary>
    /// Selects the best result from a group of similar places
    /// </summary>
    private static LocationSuggestionModel SelectBestResult(List<LocationSuggestionModel> similarPlaces)
    {
        if (similarPlaces == null || similarPlaces.Count == 0)
            return null; // Return null instead of creating a placeholder

        if (similarPlaces.Count == 1)
            return similarPlaces[0];

        // Ensure place type is not null for comparison
        foreach (var place in similarPlaces)
        {
            if (string.IsNullOrEmpty(place.PlaceType))
                place.PlaceType = "unknown";

            if (string.IsNullOrEmpty(place.AddressType))
                place.AddressType = "unknown";
        }

        // Prefer non-administrative places
        var nonAdministrative = similarPlaces
            .Where(p => !p.PlaceType.Equals("administrative", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (nonAdministrative.Count > 0)
            return nonAdministrative[0];

        // If all are administrative, prefer city addressType over municipality
        var cityAddressTypes = similarPlaces
            .Where(p => p.AddressType.Equals("city", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (cityAddressTypes.Count > 0)
            return cityAddressTypes[0];

        // Otherwise, return the first one
        return similarPlaces[0];
    }

    // Internal classes for JSON deserialization
    private class NominatimSearchResult
    {
        public string PlaceId { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty;
        public NominatimAddress? Address { get; set; }
    }

    private class NominatimAddress
    {
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? Suburb { get; set; }
        public string? Neighbourhood { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
    }
}