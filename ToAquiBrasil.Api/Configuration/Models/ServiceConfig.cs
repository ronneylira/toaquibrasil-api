namespace ToAquiBrasil.Api.Configuration.Models;

public class ServiceConfig
{
    public CountriesNowApiConfiguration CountriesNowApi { get; init; }
    
    public BigDataCloudApiConfiguration BigDataCloudApi { get; init; }
    
    public AtlasApiConfig AtlasApi { get; init; }
}