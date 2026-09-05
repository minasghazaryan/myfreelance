namespace MyFreelance.Web.Services;

public record ClientLocationInfo(string? IpAddress, string? CountryCode, string? CountryName);

public interface IClientLocationService
{
    Task<ClientLocationInfo> GetAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}
