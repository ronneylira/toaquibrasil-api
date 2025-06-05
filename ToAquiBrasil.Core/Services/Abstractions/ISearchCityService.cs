using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Core.Services.Abstractions;

public interface ISearchCityService
{
    Task<IEnumerable<CityModel>> SearchCityAsync(string cityName, string country, CancellationToken cancellationToken);
}