using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Core.Services.Abstractions;

public interface IGeocodingService
{
    /// <summary>
    /// Gets coordinates for a city name
    /// </summary>
    /// <param name="cityName">Name of the city</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Coordinates of the city</returns>
    Task<LocationModel> GetCoordinatesByCityAsync(string cityName, CancellationToken cancellationToken);

    /// <summary>
    /// Suggests location matches based on user input
    /// </summary>
    /// <param name="query">User input query</param>
    /// <param name="limit">Maximum number of suggestions to return</param>
    /// <param name="countryCode">Optional country code (ISO 3166-1 alpha-2) to filter results</param>
    /// <param name="placeTypes">Optional list of place types to filter results (e.g., city, town)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of location suggestions</returns>
    Task<List<LocationSuggestionModel>> SuggestLocationsAsync(string query, int limit = 5, string? countryCode = null, List<string>? placeTypes = null, CancellationToken cancellationToken = default);
}