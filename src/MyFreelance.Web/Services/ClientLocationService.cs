using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyFreelance.Web.Services;

public class ClientLocationService(IHttpClientFactory httpClientFactory, ILogger<ClientLocationService> logger) : IClientLocationService
{
    public async Task<ClientLocationInfo> GetAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var ip = GetClientIp(httpContext);
        var countryCode = NormalizeCountryCode(httpContext.Request.Headers["CF-IPCountry"].ToString());
        var countryName = CountryNameFromCode(countryCode);

        if (string.IsNullOrWhiteSpace(countryName) && IsPublicIp(ip))
        {
            try
            {
                var lookup = await LookupCountryAsync(ip!, cancellationToken);
                if (lookup is { } found)
                {
                    countryCode ??= found.CountryCode;
                    countryName = found.CountryName;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Country lookup failed for {Ip}", ip);
            }
        }

        return new ClientLocationInfo(ip, countryCode, countryName);
    }

    private async Task<(string CountryCode, string CountryName)?> LookupCountryAsync(string ip, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("IpLookup");
        using var response = await client.GetAsync($"http://ip-api.com/json/{Uri.EscapeDataString(ip)}?fields=status,country,countryCode", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<IpApiResponse>(stream, cancellationToken: cancellationToken);
        if (payload is null || !string.Equals(payload.Status, "success", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(payload.Country))
        {
            return null;
        }

        var code = NormalizeCountryCode(payload.CountryCode) ?? string.Empty;
        return (code, payload.Country);
    }

    private static string? GetClientIp(HttpContext httpContext)
    {
        var cfIp = FirstHeaderValue(httpContext, "CF-Connecting-IP");
        if (IsPublicIp(cfIp))
            return cfIp;

        var forwarded = FirstHeaderValue(httpContext, "X-Forwarded-For");
        if (IsPublicIp(forwarded))
            return forwarded;

        var remote = httpContext.Connection.RemoteIpAddress;
        if (remote is null)
            return null;

        if (remote.IsIPv4MappedToIPv6)
            remote = remote.MapToIPv4();

        var ip = remote.ToString();
        return string.IsNullOrWhiteSpace(ip) ? null : ip;
    }

    private static string? FirstHeaderValue(HttpContext httpContext, string headerName)
    {
        var raw = httpContext.Request.Headers[headerName].ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var first = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? null : first;
    }

    private static bool IsPublicIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip) || !IPAddress.TryParse(ip, out var address))
            return false;

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        return !IPAddress.IsLoopback(address) && !IsPrivate(address);
    }

    private static bool IsPrivate(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168)
                || bytes[0] == 127;
        }

        return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6UniqueLocal;
    }

    private static string? NormalizeCountryCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        code = code.Trim().ToUpperInvariant();
        return code is "XX" or "T1" or "A1" or "A2" ? null : code;
    }

    private static string? CountryNameFromCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
            return null;

        try
        {
            return new RegionInfo(code).EnglishName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private sealed class IpApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }
}
