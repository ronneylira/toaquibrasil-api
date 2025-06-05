namespace ToAquiBrasil.Api.Dtos;

public record RadiusDto
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}