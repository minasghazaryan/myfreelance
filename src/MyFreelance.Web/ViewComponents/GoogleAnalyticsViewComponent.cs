using Microsoft.AspNetCore.Mvc;
using MyFreelance.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MyFreelance.Web.ViewComponents;

public class GoogleAnalyticsViewComponent(ICmsService cmsService) : ViewComponent
{
    private static readonly Regex MeasurementIdPattern = new(@"^(G|GT|UA)-[A-Z0-9-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var path = HttpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return Content(string.Empty);

        var measurementId = (await cmsService.GetSiteSettingAsync("Analytics.GoogleMeasurementId"))?.Trim();
        if (string.IsNullOrWhiteSpace(measurementId) || !MeasurementIdPattern.IsMatch(measurementId))
            return Content(string.Empty);

        return View(new GoogleAnalyticsViewModel(measurementId.ToUpperInvariant()));
    }
}

public record GoogleAnalyticsViewModel(string MeasurementId);
