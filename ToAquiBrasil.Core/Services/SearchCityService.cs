using System.Net.Http.Json;
using System.Text.Json.Nodes;
using ToAquiBrasil.Core.Models;
using ToAquiBrasil.Core.Services.Abstractions;
using ToAquiBrasil.Core.Services.Dtos;

namespace ToAquiBrasil.Core.Services;

public class SearchCityService(HttpClient httpClient) : ISearchCityService
{
    private readonly Uri _searchCityUri = new ("countries/cities", UriKind.Relative);
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<IEnumerable<CityModel>> SearchCityAsync(string cityName, string country,
        CancellationToken cancellationToken)
    {
        var content = new JsonObject
        {
            ["country"] = country
        };

        var response = await _httpClient.PostAsync(_searchCityUri, JsonContent.Create(content), cancellationToken);

        //TODO: handle error
        if (!response.IsSuccessStatusCode) return [];

        var result = await response.Content.ReadFromJsonAsync<CountriesNowCityResponseDto>(cancellationToken);
        
        //TODO: handle error
        if (result!.Error) return [];
        
        return result.Data
            .Where(d => d.Contains(cityName, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(cityName))
            .Select(city => new CityModel {Name = city});
    }
}