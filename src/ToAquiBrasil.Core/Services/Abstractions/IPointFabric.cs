using NetTopologySuite.Geometries;

namespace ToAquiBrasil.Core.Services.Abstractions;

public interface IPointFabric
{
    Point Create(double latitude, double longitude);
}
