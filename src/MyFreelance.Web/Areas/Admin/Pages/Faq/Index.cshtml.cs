using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Faq;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IList<FaqItem> Items { get; set; } = [];
    [BindProperty] public FaqItem Input { get; set; } = new();

    public async Task OnGetAsync() => Items = await db.FaqItems.OrderBy(f => f.SortOrder).ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        db.FaqItems.Add(Input);
        await db.SaveChangesAsync();
        return RedirectToPage();
    }
}
