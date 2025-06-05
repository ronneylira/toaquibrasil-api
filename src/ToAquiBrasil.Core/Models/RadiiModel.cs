namespace ToAquiBrasil.Core.Models;

public class RadiiModel(RadiusModel[] radii)
{
    public RadiusModel[] Radii { get; init; } = radii;
}