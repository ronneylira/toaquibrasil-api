namespace ToAquiBrasil.Api.Dtos;

public record FeaturesDto
{
    public string Type { get; init; } = string.Empty;
    public required GeometryDto Geometry { get; init; }
    public required PropertiesDto Properties { get; init; }
}