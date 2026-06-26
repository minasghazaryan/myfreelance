using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Users;

public class IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext db) : PageModel
{
    public IList<UserItem> Users { get; set; } = [];
    public record UserItem(string Id, string Name, string Email, bool IsSuspended, bool IsKycApproved, DateTime CreatedAt);

    public async Task OnGetAsync(string? search)
    {
        var query = userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email!.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search));

        Users = await query.OrderByDescending(u => u.CreatedAt)
            .Take(100)
            .Select(u => new UserItem(u.Id, u.FullName, u.Email!, u.IsSuspended, u.IsKycApproved, u.CreatedAt))
            .ToListAsync();
    }
}
