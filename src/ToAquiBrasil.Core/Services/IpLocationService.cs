using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ToAquiBrasil.Core.Models;
using ToAquiBrasil.Core.Services.Abstractions;
using ToAquiBrasil.Core.Services.Dtos;

namespace ToAquiBrasil.Core.Services;

public class IpLocationService(HttpClient httpClient, IIpLocationServiceConfig config, ILogger<IpLocationService> logger) : IIpLocationService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly IIpLocationServiceConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ILogger<IpLocationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IpLocationModel> GetIpInfoAsync(string? clientIp, CancellationToken cancellationToken)
    {
        try
        {
            // 3. Try to get IP from the external IP lookup service
            // NOTE: This makes an external API call to retrieve the server's public IP
            // Only do this as a last resort in development environments
            if (string.IsNullOrWhiteSpace(clientIp) || clientIp == "127.0.0.1" || clientIp == "::1")
            {
                clientIp = await RetryIpRetrievalExternalAsync(cancellationToken);
            }

            // If we still don't have a valid IP, default to São Paulo
            if (string.IsNullOrWhiteSpace(clientIp))
            {
                _logger.LogWarning("All IP detection methods failed, using default location");
                return new IpLocationModel(clientIp, "Brazil", "São Paulo", -23.5505, -46.6333);
            }

            _logger.LogInformation("Using IP for geolocation: {ClientIp}", clientIp);
            var location = await GetIpInfoCoreAsync(clientIp, cancellationToken);
            var normalizedCountryName = NormalizeCountryName(location.Country.Name);
            return new IpLocationModel(clientIp, normalizedCountryName, location.Location.City, location.Location.Latitude,location.Location.Longitude);
        }
        catch (Exception ex)
        {
            // If anything goes wrong, return a default location
            _logger.LogError(ex, "Error getting IP location");
            return new IpLocationModel(clientIp, "Brazil", "São Paulo", -23.5505, -46.6333);
        }
    }

    /// <summary>
    /// Normalizes country names by removing common prefixes and suffixes
    /// </summary>
    /// <param name="countryName">The original country name</param>
    /// <returns>Normalized country name</returns>
    private static string NormalizeCountryName(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
            return countryName;

        var normalized = countryName.Trim();

        // Handle parenthetical governmental types that appear at the end
        // e.g., "Netherlands (Kingdom of the)" -> "Netherlands"
        var parenthesesPattern = @"\s*\([^)]*\)\s*$";
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, parenthesesPattern, "");

        // Remove common prefixes (case-insensitive) - in case some APIs still use this format
        var prefixesToRemove = new[]
        {
            "Kingdom of ",
            "Republic of ",
            "Democratic Republic of ",
            "People's Republic of ",
            "Federal Republic of ",
            "Islamic Republic of ",
            "United Kingdom of ",
            "Commonwealth of ",
            "Federation of ",
            "Union of ",
            "State of ",
            "Principality of ",
            "Sultanate of ",
            "Emirate of ",
            "Grand Duchy of ",
            "Duchy of "
        };

        foreach (var prefix in prefixesToRemove)
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(prefix.Length);
                break; // Only remove the first match
            }
        }

        // Handle specific well-known cases
        var countryMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "United States of America", "United States" },
            { "USA", "United States" },
            { "US", "United States" },
            { "UK", "United Kingdom" },
            { "Great Britain", "United Kingdom" },
            { "Russian Federation", "Russia" },
            { "South Korea", "Korea" },
            { "North Korea", "Korea" },
            { "Czech Republic", "Czechia" },
            { "Bosnia and Herzegovina", "Bosnia" },
            { "Trinidad and Tobago", "Trinidad" },
            { "Saint Vincent and the Grenadines", "Saint Vincent" },
            { "Antigua and Barbuda", "Antigua" },
            { "São Tomé and Príncipe", "São Tomé" },
            { "Saint Kitts and Nevis", "Saint Kitts" }
        };

        if (countryMappings.TryGetValue(normalized, out var mappedName))
        {
            normalized = mappedName;
        }

        return normalized.Trim();
    }

    private async Task<string?> RetryIpRetrievalExternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Try multiple IP detection services in case one is down
            var ipDetectionServices = new[]
            {
                "https://api.ipify.org",
                "https://icanhazip.com",
                "https://ifconfig.me/ip"
            };

            foreach (var service in ipDetectionServices)
            {
                try
                {
                    // Make a request to the IP detection service
                    var response = await httpClient.GetStringAsync(service, cancellationToken);
                    var detectedIp = response.Trim();

                    // Validate that we got a proper IP address
                    if (!string.IsNullOrWhiteSpace(detectedIp) &&
                        System.Net.IPAddress.TryParse(detectedIp, out _))
                    {
                        _logger.LogInformation("Determined IP using external service {Service}: {ClientIp}", service, detectedIp);
                        return detectedIp;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get IP from {Service}", service);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get IP from external services");
        }

        return null;
    }


    private async Task<IpLocationResponseDto?> GetIpInfoCoreAsync(string? ip, CancellationToken cancellationToken)
    {
        var uri = new Uri($"data/ip-geolocation?ip={ip}&key={_config.ApiKey}", UriKind.Relative);
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<IpLocationResponseDto?>(cancellationToken);
    }
}
