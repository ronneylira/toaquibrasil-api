namespace ToAquiBrasil.Api.Dtos;

public record LocationSuggestionDto
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string PlaceType { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string CountryCode { get; init; } = string.Empty;
    public string AddressType { get; init; } = string.Empty;
} 