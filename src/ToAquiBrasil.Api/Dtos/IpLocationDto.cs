namespace ToAquiBrasil.Api.Dtos;

public record IpLocationDto
{
    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}