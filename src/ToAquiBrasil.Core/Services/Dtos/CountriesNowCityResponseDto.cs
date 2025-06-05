namespace ToAquiBrasil.Core.Services.Dtos;

internal class CountriesNowCityResponseDto
{
    public bool Error { get; init; }
    public string Msg { get; init; }
    public string[] Data { get; init; }
}