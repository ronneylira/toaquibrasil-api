namespace ToAquiBrasil.Api.Dtos;

public record LocationDto
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? DisplayName { get; init; }
} 