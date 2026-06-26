using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Cms;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IList<CmsPage> Pages { get; set; } = [];
    [BindProperty] public CmsPage Input { get; set; } = new();

    public async Task OnGetAsync() => Pages = await db.CmsPages.ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        db.CmsPages.Add(Input);
        await db.SaveChangesAsync();
        return RedirectToPage();
    }
}
