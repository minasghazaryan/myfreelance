using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Middleware;

public class GhanaVisitorGateMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    private const string PromoHost = RegistrationSources.PromoHost;
    private static readonly string FillerPage = ReadFillerPage();

    private static string ReadFillerPage()
    {
        var assembly = typeof(GhanaVisitorGateMiddleware).Assembly;
        using var stream = assembly.GetManifestResourceStream("MyFreelance.Web.Middleware.astrology-forum.html")
            ?? throw new InvalidOperationException("Astrology forum page is missing from the application.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static readonly HashSet<string> AllowedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "GH",
        "AM",
        "US"
    };

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

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store";
        AppendVary(context);
        await context.Response.WriteAsync(FillerPage);
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

        return AllowedCountries.Contains(country);
    }

    private static void AppendVary(HttpContext context)
    {
        var vary = context.Response.Headers.Vary.ToString();
        if (vary.Contains("CF-IPCountry", StringComparison.OrdinalIgnoreCase))
            return;

        context.Response.Headers.Append("Vary", "CF-IPCountry");
    }
}
