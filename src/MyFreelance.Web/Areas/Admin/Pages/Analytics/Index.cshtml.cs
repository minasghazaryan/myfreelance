using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;
using System.Text.RegularExpressions;

namespace MyFreelance.Web.Areas.Admin.Pages.Analytics;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    private static readonly Regex MeasurementIdPattern = new(@"^(G|GT|UA)-[A-Z0-9-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [BindProperty]
    public string MeasurementId { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        MeasurementId = await db.SiteSettings
            .Where(s => s.Key == "Analytics.GoogleMeasurementId")
            .Select(s => s.Value)
            .FirstOrDefaultAsync() ?? string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        var value = MeasurementId?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(value) && !MeasurementIdPattern.IsMatch(value))
        {
            TempData["ErrorMessage"] = "Enter a valid Google tag ID, for example G-XXXXXXXXXX.";
            return RedirectToPage();
        }

        var setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Analytics.GoogleMeasurementId");
        if (setting is null)
        {
            db.SiteSettings.Add(new SiteSettings
            {
                Key = "Analytics.GoogleMeasurementId",
                Value = value,
                Category = "Analytics",
                Description = "Google Analytics 4 measurement ID"
            });
        }
        else
        {
            setting.Value = value;
        }

        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = string.IsNullOrWhiteSpace(value)
            ? "Google Analytics disabled."
            : "Google Analytics measurement ID saved.";
        return RedirectToPage();
    }
}
