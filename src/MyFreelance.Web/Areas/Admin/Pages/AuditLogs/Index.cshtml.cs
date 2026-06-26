using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.AuditLogs;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IList<AuditLog> Logs { get; set; } = [];
    public async Task OnGetAsync() => Logs = await db.AuditLogs.OrderByDescending(l => l.CreatedAt).Take(200).ToListAsync();
}
