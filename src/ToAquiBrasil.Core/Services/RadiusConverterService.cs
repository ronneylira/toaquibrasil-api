using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Core.Services;

public class RadiusConverterService : IRadiusConverterService
{
    private const double KilometersToMeters = 1000;
    private const double MilesToMeters = 1609;
    public double ConvertRadiusToMeters(int radius, string unit) =>
        unit.ToLowerInvariant() switch
        {
            "km" => radius * KilometersToMeters,
            "mi" => radius * MilesToMeters,
            _ => throw new ArgumentException($"Invalid unit: {unit}. Supported units are 'km' and 'mi'.")
        };
}
