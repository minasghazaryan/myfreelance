using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Tiers;

public class IndexModel(ApplicationDbContext db, IFileStorageService fileStorage) : PageModel
{
    public IList<InvestmentTier> Tiers { get; set; } = [];

    [BindProperty] public InvestmentTier Input { get; set; } = new() { RiskLevel = RiskLevel.Low, IsActive = true };

    [BindProperty] public IFormFile? Photo { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        ErrorMessage = TempData["ErrorMessage"] as string;
        Tiers = await db.InvestmentTiers.OrderBy(t => t.SortOrder).ThenBy(t => t.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        if (Input.RiskLevel == 0)
            Input.RiskLevel = RiskLevel.Low;

        if (Input.MinInvestment > Input.MaxInvestment)
        {
            TempData["ErrorMessage"] = "Minimum investment cannot be greater than maximum.";
            return RedirectToPage();
        }

        try
        {
            Input.Name = Input.Name.Trim();
            Input.SortOrder = (await db.InvestmentTiers.MaxAsync(t => (int?)t.SortOrder) ?? 0) + 1;
            Input.IsActive = true;
            Input.ImagePath = await TierPhotoHelper.SaveAsync(fileStorage, Photo);
            Input.PromoBannerText = string.IsNullOrWhiteSpace(Input.PromoBannerText) ? null : Input.PromoBannerText.Trim();
            Input.AccentColor = string.IsNullOrWhiteSpace(Input.AccentColor) ? null : Input.AccentColor.Trim();
            Input.IconClass = string.IsNullOrWhiteSpace(Input.IconClass) ? null : Input.IconClass.Trim();

            db.InvestmentTiers.Add(Input);
            await db.SaveChangesAsync();
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        var tier = await db.InvestmentTiers.FindAsync(id);
        if (tier is not null)
        {
            await TierPhotoHelper.DeleteUploadedAsync(fileStorage, tier.ImagePath);
            db.InvestmentTiers.Remove(tier);
            await db.SaveChangesAsync();
            await NormalizeSortOrderAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(Guid id, string direction)
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        var ordered = await db.InvestmentTiers
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        var index = ordered.FindIndex(t => t.Id == id);
        if (index < 0)
            return RedirectToPage();

        var targetIndex = direction == "up" ? index - 1 : index + 1;
        if (targetIndex < 0 || targetIndex >= ordered.Count)
            return RedirectToPage();

        var current = ordered[index];
        var target = ordered[targetIndex];
        (current.SortOrder, target.SortOrder) = (target.SortOrder, current.SortOrder);

        await db.SaveChangesAsync();
        await NormalizeSortOrderAsync();
        return RedirectToPage();
    }

    private async Task NormalizeSortOrderAsync()
    {
        var ordered = await db.InvestmentTiers
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SortOrder = i + 1;

        await db.SaveChangesAsync();
    }
}
