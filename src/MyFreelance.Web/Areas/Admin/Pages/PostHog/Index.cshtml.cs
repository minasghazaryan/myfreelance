using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;
using System.Text.RegularExpressions;

namespace MyFreelance.Web.Areas.Admin.Pages.PostHog;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    private static readonly Regex ApiKeyPattern = new(@"^phc_[A-Za-z0-9]+$", RegexOptions.Compiled);
    private static readonly Regex ApiHostPattern = new(@"^https://[A-Za-z0-9.-]+(?:/[A-Za-z0-9._~/-]*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private const string DefaultApiHost = "https://us.i.posthog.com";

    [BindProperty]
    public string ApiKey { get; set; } = string.Empty;

    [BindProperty]
    public string ApiHost { get; set; } = DefaultApiHost;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        ApiKey = await GetSettingAsync("Analytics.PostHogApiKey") ?? string.Empty;
        ApiHost = await GetSettingAsync("Analytics.PostHogApiHost") ?? DefaultApiHost;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        var key = ApiKey?.Trim() ?? string.Empty;
        var host = string.IsNullOrWhiteSpace(ApiHost) ? DefaultApiHost : ApiHost.Trim().TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(key) && !ApiKeyPattern.IsMatch(key))
        {
            TempData["ErrorMessage"] = "Enter a valid PostHog project API key (starts with phc_).";
            return RedirectToPage();
        }

        if (!ApiHostPattern.IsMatch(host))
        {
            TempData["ErrorMessage"] = "Enter a valid HTTPS API host, for example https://us.i.posthog.com or https://eu.i.posthog.com.";
            return RedirectToPage();
        }

        await UpsertSettingAsync("Analytics.PostHogApiKey", key, "PostHog project API key");
        await UpsertSettingAsync("Analytics.PostHogApiHost", host, "PostHog API host");
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = string.IsNullOrWhiteSpace(key)
            ? "PostHog disabled."
            : "PostHog settings saved.";
        return RedirectToPage();
    }

    private async Task<string?> GetSettingAsync(string key) =>
        await db.SiteSettings.Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync();

    private async Task UpsertSettingAsync(string key, string value, string description)
    {
        var setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            db.SiteSettings.Add(new SiteSettings
            {
                Key = key,
                Value = value,
                Category = "Analytics",
                Description = description
            });
        }
        else
        {
            setting.Value = value;
        }
    }
}
