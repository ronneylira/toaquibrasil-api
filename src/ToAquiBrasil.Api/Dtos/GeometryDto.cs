using System.Text.Json.Serialization;

namespace ToAquiBrasil.Api.Dtos;

public record GeometryDto
{
    public string Type { get; init; } = string.Empty;
    public double[] Coordinates => new[] {Longitude, Latitude};
    [JsonIgnore]
    public double Latitude { get; init; }
    [JsonIgnore]
    public double Longitude { get; init; }
}