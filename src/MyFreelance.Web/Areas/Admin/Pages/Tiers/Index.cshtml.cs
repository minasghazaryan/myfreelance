using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Tiers;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IList<InvestmentTier> Tiers { get; set; } = [];

    [BindProperty] public InvestmentTier Input { get; set; } = new() { RiskLevel = RiskLevel.Low, IsActive = true };

    public async Task OnGetAsync() => Tiers = await db.InvestmentTiers.OrderBy(t => t.SortOrder).ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.RiskLevel == 0)
            Input.RiskLevel = RiskLevel.Low;

        Input.SortOrder = await db.InvestmentTiers.CountAsync() + 1;
        Input.IsActive = true;

        db.InvestmentTiers.Add(Input);
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var tier = await db.InvestmentTiers.FindAsync(id);
        if (tier is not null) { db.InvestmentTiers.Remove(tier); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
