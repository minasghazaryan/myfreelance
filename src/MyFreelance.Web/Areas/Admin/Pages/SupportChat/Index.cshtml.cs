using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.SupportChat;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public SupportChatSettings? Settings { get; set; }
    [BindProperty] public SupportChatSettings Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Settings = await db.SupportChatSettings.OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
        if (Settings is not null) Input = Settings;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await db.SupportChatSettings.FirstOrDefaultAsync();
        if (existing is not null)
        {
            existing.ScriptContent = Input.ScriptContent;
            existing.IsEnabled = Input.IsEnabled;
            existing.ShowOnLanding = Input.ShowOnLanding;
            existing.ShowOnDashboard = Input.ShowOnDashboard;
        }
        else db.SupportChatSettings.Add(Input);
        await db.SaveChangesAsync();
        return RedirectToPage();
    }
}
