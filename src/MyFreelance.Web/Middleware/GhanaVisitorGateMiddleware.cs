using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Middleware;

public class GhanaVisitorGateMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    private const string PromoHost = "promo.africa-usainvest.com";
    private const string OutsideGhanaRedirectUrl = "https://www.google.com/";

    private static readonly string[] AllowedPaths =
    [
        "/account/login",
        "/account/logout",
        "/account/accessdenied",
        "/account/forgotpassword",
        "/account/forgotpasswordconfirmation"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsPromoHost(context))
        {
            await next(context);
            return;
        }

        if (ShouldShowSite(context))
        {
            context.Response.OnStarting(() =>
            {
                AppendVary(context);
                return Task.CompletedTask;
            });
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status302Found;
        context.Response.Headers.Location = OutsideGhanaRedirectUrl;
        context.Response.Headers.CacheControl = "no-store";
        AppendVary(context);
    }

    private static bool IsPromoHost(HttpContext context) =>
        context.Request.Host.Host.Equals(PromoHost, StringComparison.OrdinalIgnoreCase);

    private bool ShouldShowSite(HttpContext context)
    {
        if (context.User.IsInRole(AppRoles.Admin) || context.User.IsInRole(AppRoles.AdminReadOnly))
            return true;

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var allowed in AllowedPaths)
        {
            if (path.Equals(allowed, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var country = context.Request.Headers["CF-IPCountry"].ToString().Trim();
        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(country))
            return true;

        return country.Equals("GH", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendVary(HttpContext context)
    {
        var vary = context.Response.Headers.Vary.ToString();
        if (vary.Contains("CF-IPCountry", StringComparison.OrdinalIgnoreCase))
            return;

        context.Response.Headers.Append("Vary", "CF-IPCountry");
    }
}
