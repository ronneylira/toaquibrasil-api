namespace ToAquiBrasil.Api.Dtos;

public record CitiesDto
{
    public IEnumerable<string> Cities { get; init; } = [];
}