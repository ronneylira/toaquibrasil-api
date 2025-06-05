using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Core.Queries.Abstractions;

public interface ILayoutQueries
{
    Task<LocationBasedLayoutModel> GetLocationBasedLayoutAsync(double latitude, double longitude, int radius,
        string unit, CancellationToken cancellationToken);
}
