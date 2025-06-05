namespace ToAquiBrasil.Api.Dtos;

public record CategoryDto
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}