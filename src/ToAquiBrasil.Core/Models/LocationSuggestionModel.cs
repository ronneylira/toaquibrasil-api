namespace ToAquiBrasil.Core.Models;

public class LocationSuggestionModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PlaceType { get; set; } = string.Empty; // city, state, country, etc.
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string AddressType { get; set; } = string.Empty; // The specific type within OpenStreetMap
} 