namespace ToAquiBrasil.Core.Services.Abstractions;

public interface IIpLocationServiceConfig
{
    string BaseUrl { get; }
    string ApiKey { get; }
}