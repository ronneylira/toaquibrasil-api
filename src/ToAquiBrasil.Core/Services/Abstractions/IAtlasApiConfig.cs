namespace ToAquiBrasil.Core.Services.Abstractions;

public interface IAtlasApiConfig
{
    string BaseUrl { get; }
    
    string ApiKey { get; }
}