using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Users;

public class DetailsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext db) : PageModel
{
    public ApplicationUser Account { get; set; } = null!;
    public UserWallet? Wallet { get; set; }
    public string Role { get; set; } = "—";
    public bool IsInvestor { get; set; }
    public string? ReferredBy { get; set; }
    public int InvestmentCount { get; set; }
    public int DepositCount { get; set; }
    public int WithdrawalCount { get; set; }
    public bool CanManageUsers => User.IsInRole(AppRoles.Admin);

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var account = await userManager.Users
            .Include(u => u.ReferredBy)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (account is null)
            return NotFound();

        Account = account;
        Wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == id);
        InvestmentCount = await db.Investments.CountAsync(i => i.UserId == id);
        DepositCount = await db.Deposits.CountAsync(d => d.UserId == id);
        WithdrawalCount = await db.Withdrawals.CountAsync(w => w.UserId == id);

        var roles = await userManager.GetRolesAsync(account);
        IsInvestor = roles.Contains(AppRoles.Investor)
            && !roles.Contains(AppRoles.Admin)
            && !roles.Contains(AppRoles.AdminReadOnly);
        Role = roles.Contains(AppRoles.Admin) ? "Full Admin"
            : roles.Contains(AppRoles.AdminReadOnly) ? "Read-Only Admin"
            : IsInvestor ? "Investor"
            : roles.FirstOrDefault() ?? "—";

        ReferredBy = account.ReferredBy is null
            ? null
            : $"{account.ReferredBy.FullName} ({account.ReferredBy.Email})";

        return Page();
    }
}
