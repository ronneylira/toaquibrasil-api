namespace ToAquiBrasil.Core.Services.Abstractions;

public interface IRadiusConverterService
{
    double ConvertRadiusToMeters(int radius, string unit);
}
