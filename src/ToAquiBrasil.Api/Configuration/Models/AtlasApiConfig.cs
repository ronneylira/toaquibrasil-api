using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Api.Configuration.Models;

public class AtlasApiConfig : IAtlasApiConfig
{
    public string BaseUrl { get; init; }
    
    public string ApiKey { get; init; }
}