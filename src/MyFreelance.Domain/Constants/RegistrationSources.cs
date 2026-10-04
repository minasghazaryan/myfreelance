namespace MyFreelance.Domain.Constants;

public static class RegistrationSources
{
    public const string Promo = "Promo";
    public const string Direct = "Direct";
    public const string Admin = "Admin";
    public const string PromoHost = "promo.africa-usainvest.com";

    public static string FromHost(string? host) =>
        string.Equals(host?.Trim(), PromoHost, StringComparison.OrdinalIgnoreCase)
            ? Promo
            : Direct;
}
