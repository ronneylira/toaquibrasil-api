using Riok.Mapperly.Abstractions;
using ToAquiBrasil.Api.Dtos;
using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Api.Mappers;

[Mapper]
public partial class CitiesMapper
{
    public CitiesDto MapToCitiesDto(IEnumerable<CityModel> cities) => new()
    {
        Cities = cities.Select(c => c.Name)
    };
}