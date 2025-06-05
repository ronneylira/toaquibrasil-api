using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Core.Services.Abstractions;

public interface IIpLocationService
{
    Task<IpLocationModel> GetIpInfoAsync(string? clientIp, CancellationToken cancellationToken);
}