using NetTopologySuite.Geometries;
using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Core.Services;

public class PointFabric : IPointFabric
{
    private const int SridStandardForGps = 4326;
    
    public Point Create(double latitude, double longitude)
    {
        return new Point(longitude, latitude) { SRID = SridStandardForGps };
    }
}
