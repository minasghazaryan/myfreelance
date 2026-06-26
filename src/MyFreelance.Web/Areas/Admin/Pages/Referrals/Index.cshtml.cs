using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Referrals;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IList<ReferralConfig> Configs { get; set; } = [];
    [BindProperty] public ReferralConfig Input { get; set; } = new();

    public async Task OnGetAsync() => Configs = await db.ReferralConfigs.OrderBy(c => c.Level).ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await db.ReferralConfigs.FirstOrDefaultAsync(c => c.Level == Input.Level);
        if (existing is not null) { existing.Percentage = Input.Percentage; existing.Description = Input.Description; }
        else db.ReferralConfigs.Add(Input);
        await db.SaveChangesAsync();
        return RedirectToPage();
    }
}
