namespace ToAquiBrasil.Core.Queries.Abstractions;

public class GetListingsFilterModel
{
    public string? Category { get; init; }
    public string? Keyword { get; init; }
    public IEnumerable<string>? Tags { get; init; }
}
