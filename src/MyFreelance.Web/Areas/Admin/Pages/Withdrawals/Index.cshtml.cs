using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Withdrawals;

public class IndexModel(ApplicationDbContext db, IWithdrawalService withdrawalService, UserManager<ApplicationUser> userManager) : PageModel
{
    public IList<WithdrawalItem> Withdrawals { get; set; } = [];
    public record WithdrawalItem(Guid Id, string UserEmail, decimal Amount, string Status, DateTime CreatedAt);

    public async Task OnGetAsync()
    {
        Withdrawals = await db.Withdrawals.Include(w => w.User).OrderByDescending(w => w.CreatedAt).Take(100)
            .Select(w => new WithdrawalItem(w.Id, w.User.Email!, w.Amount, w.Status.ToString(), w.CreatedAt)).ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        await withdrawalService.ApproveWithdrawalAsync(id, userManager.GetUserId(User)!);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        await withdrawalService.RejectWithdrawalAsync(id, userManager.GetUserId(User)!, "Rejected by admin");
        return RedirectToPage();
    }
}
