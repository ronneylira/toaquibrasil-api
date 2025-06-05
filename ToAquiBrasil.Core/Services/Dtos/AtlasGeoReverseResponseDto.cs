namespace ToAquiBrasil.Core.Services.Dtos;

public record AtlasGeoReverseResponseDto(
    string type,
    Features[] features
);

public record Features(
    string type,
    Properties properties,
    Geometry geometry,
    double[] bbox
);

public record Properties(
    Address address,
    string type,
    string confidence,
    string[] matchCodes,
    GeocodePoints[] geocodePoints
);

public record Address(
    CountryRegion countryRegion,
    AdminDistricts[] adminDistricts,
    string formattedAddress,
    string locality,
    string postalCode,
    string addressLine
);

public record CountryRegion(
    string name
);

public record AdminDistricts(
    string shortName
);

public record GeocodePoints(
    Geometry geometry,
    string calculationMethod,
    string[] usageTypes
);

public record Geometry(
    string type,
    double[] coordinates
);