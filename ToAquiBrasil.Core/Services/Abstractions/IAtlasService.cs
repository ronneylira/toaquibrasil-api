using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Core.Services.Abstractions;

public interface IGeoReverseService
{
    Task<CityModel> GetGeoReverseAsync(double latitude, double longitude, CancellationToken cancellationToken);
}