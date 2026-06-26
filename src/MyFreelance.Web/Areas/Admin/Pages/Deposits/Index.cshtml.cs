using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Deposits;

public class IndexModel(ApplicationDbContext db, IDepositService depositService, UserManager<ApplicationUser> userManager) : PageModel
{
    public IList<DepositItem> Deposits { get; set; } = [];
    public record DepositItem(Guid Id, string UserEmail, decimal Amount, string Status, DateTime CreatedAt);

    public async Task OnGetAsync()
    {
        Deposits = await db.Deposits.Include(d => d.User).OrderByDescending(d => d.CreatedAt).Take(100)
            .Select(d => new DepositItem(d.Id, d.User.Email!, d.Amount, d.Status.ToString(), d.CreatedAt)).ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        await depositService.ConfirmDepositAsync(id, userManager.GetUserId(User)!);
        return RedirectToPage();
    }
}
