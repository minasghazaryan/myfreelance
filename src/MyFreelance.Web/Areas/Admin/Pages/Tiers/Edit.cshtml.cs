using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Tiers;

public class EditModel(ApplicationDbContext db, IFileStorageService fileStorage) : PageModel
{
    public InvestmentTier Tier { get; set; } = new();

    [BindProperty]
    public InvestmentTier Input { get; set; } = new();

    [BindProperty]
    public IFormFile? Photo { get; set; }

    [BindProperty]
    public bool RemovePhoto { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        var tier = await db.InvestmentTiers.FindAsync(id);
        if (tier is null)
            return NotFound();

        Tier = tier;
        Input = CloneTier(tier);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        var tier = await db.InvestmentTiers.FindAsync(id);
        if (tier is null)
            return NotFound();

        Tier = tier;
        Input.Id = id;

        if (Input.RiskLevel == 0)
            Input.RiskLevel = RiskLevel.Low;

        if (Input.MinInvestment > Input.MaxInvestment)
        {
            ErrorMessage = "Minimum investment cannot be greater than maximum.";
            return Page();
        }

        try
        {
            if (RemovePhoto && !string.IsNullOrWhiteSpace(tier.ImagePath))
            {
                await TierPhotoHelper.DeleteUploadedAsync(fileStorage, tier.ImagePath);
                tier.ImagePath = null;
            }

            if (Photo is { Length: > 0 })
            {
                var storedPath = await TierPhotoHelper.SaveAsync(fileStorage, Photo);
                if (storedPath is not null)
                {
                    await TierPhotoHelper.DeleteUploadedAsync(fileStorage, tier.ImagePath);
                    tier.ImagePath = storedPath;
                }
            }

            tier.Name = Input.Name.Trim();
            tier.Description = Input.Description.Trim();
            tier.PackageDetails = string.IsNullOrWhiteSpace(Input.PackageDetails) ? null : Input.PackageDetails.Trim();
            tier.InsuranceNotice = string.IsNullOrWhiteSpace(Input.InsuranceNotice) ? null : Input.InsuranceNotice.Trim();
            tier.ProjectedYieldPercent = Input.ProjectedYieldPercent;
            tier.MinInvestment = Input.MinInvestment;
            tier.MaxInvestment = Input.MaxInvestment;
            tier.RiskLevel = Input.RiskLevel;
            tier.AccentColor = string.IsNullOrWhiteSpace(Input.AccentColor) ? null : Input.AccentColor.Trim();
            tier.IconClass = string.IsNullOrWhiteSpace(Input.IconClass) ? null : Input.IconClass.Trim();
            tier.PromoBannerText = string.IsNullOrWhiteSpace(Input.PromoBannerText) ? null : Input.PromoBannerText.Trim();
            tier.PromoEndUtc = Input.PromoEndUtc;
            tier.IsActive = Input.IsActive;

            await db.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    private static InvestmentTier CloneTier(InvestmentTier tier) => new()
    {
        Id = tier.Id,
        Name = tier.Name,
        Description = tier.Description,
        PackageDetails = tier.PackageDetails,
        InsuranceNotice = tier.InsuranceNotice,
        ProjectedYieldPercent = tier.ProjectedYieldPercent,
        MinInvestment = tier.MinInvestment,
        MaxInvestment = tier.MaxInvestment,
        RiskLevel = tier.RiskLevel,
        AccentColor = tier.AccentColor,
        IconClass = tier.IconClass,
        PromoBannerText = tier.PromoBannerText,
        PromoEndUtc = tier.PromoEndUtc,
        IsActive = tier.IsActive,
        ImagePath = tier.ImagePath
    };
}
