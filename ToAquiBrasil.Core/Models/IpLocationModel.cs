namespace ToAquiBrasil.Core.Models;

public class IpLocationModel(string? ip, string country, string city, double latitude, double longitude)
{
    public string? Ip { get; init; } = ip;
    public string Country { get; init; } = country;
    public string City { get; init; } = city;
    public double Latitude { get; init; } = latitude;
    public double Longitude { get; init; } = longitude;
}