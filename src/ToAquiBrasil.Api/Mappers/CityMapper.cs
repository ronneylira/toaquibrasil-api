using Riok.Mapperly.Abstractions;
using ToAquiBrasil.Api.Dtos;
using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Api.Mappers;

[Mapper]
public partial class CityMapper
{
    public partial CityDto MapToCityDto(CityModel city);
}