using ToAquiBrasil.Core.Models.Domain;

namespace ToAquiBrasil.Core.Queries.Abstractions;

public interface IListingQueries
{
    Task<ListingResults> GetListingResultsAsync(
        double latitude,
        double longitude,
        int radius,
        string unit,
        GetListingsFilterModel? filterModel = null,
        CancellationToken cancellationToken = default);

    Task<ListingResult?> GetListingResultByExternalIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default);
}