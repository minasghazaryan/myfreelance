using Microsoft.AspNetCore.Mvc;
using MyFreelance.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MyFreelance.Web.ViewComponents;

public class PostHogViewComponent(ICmsService cmsService) : ViewComponent
{
    private static readonly Regex ApiKeyPattern = new(@"^phc_[A-Za-z0-9]+$", RegexOptions.Compiled);
    private static readonly Regex ApiHostPattern = new(@"^https://[A-Za-z0-9.-]+(?:/[A-Za-z0-9._~/-]*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var path = HttpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return Content(string.Empty);

        var apiKey = (await cmsService.GetSiteSettingAsync("Analytics.PostHogApiKey"))?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey) || !ApiKeyPattern.IsMatch(apiKey))
            return Content(string.Empty);

        var apiHost = (await cmsService.GetSiteSettingAsync("Analytics.PostHogApiHost"))?.Trim();
        if (string.IsNullOrWhiteSpace(apiHost) || !ApiHostPattern.IsMatch(apiHost))
            apiHost = "https://us.i.posthog.com";

        return View(new PostHogViewModel(apiKey, apiHost.TrimEnd('/')));
    }
}

public record PostHogViewModel(string ApiKey, string ApiHost);
