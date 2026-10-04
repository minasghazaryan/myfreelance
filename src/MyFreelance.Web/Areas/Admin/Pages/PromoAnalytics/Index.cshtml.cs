using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.PromoAnalytics;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IReadOnlyList<PromoUserRow> Users { get; private set; } = [];
    public int UserCount { get; private set; }
    public decimal ConfirmedDeposits { get; private set; }
    public decimal PendingDeposits { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal Invested { get; private set; }
    public decimal Withdrawn { get; private set; }

    public async Task OnGetAsync()
    {
        Users = await db.Users
            .AsNoTracking()
            .Where(u => u.RegistrationSource == RegistrationSources.Promo)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new PromoUserRow(
                u.Id,
                (u.FirstName + " " + u.LastName).Trim(),
                u.Email ?? string.Empty,
                u.PhoneNumber,
                u.RegistrationCountry,
                u.CreatedAt,
                u.IsKycApproved,
                u.IsSuspended,
                u.Wallet == null ? 0 : u.Wallet.TotalDeposited,
                u.Deposits
                    .Where(d => d.Status == DepositStatus.Pending || d.Status == DepositStatus.Confirming)
                    .Sum(d => (decimal?)d.Amount) ?? 0,
                u.Wallet == null ? 0 : u.Wallet.AvailableBalance,
                u.Wallet == null ? 0 : u.Wallet.InvestedCapital,
                u.Wallet == null ? 0 : u.Wallet.ReferralEarnings,
                u.Wallet == null ? 0 : u.Wallet.TotalWithdrawn,
                u.Investments.Count(i => i.Status == InvestmentStatus.Active)))
            .ToListAsync();

        UserCount = Users.Count;
        ConfirmedDeposits = Users.Sum(u => u.ConfirmedDeposits);
        PendingDeposits = Users.Sum(u => u.PendingDeposits);
        AvailableBalance = Users.Sum(u => u.AvailableBalance);
        Invested = Users.Sum(u => u.Invested);
        Withdrawn = Users.Sum(u => u.Withdrawn);
    }

    public record PromoUserRow(
        string Id,
        string Name,
        string Email,
        string? PhoneNumber,
        string? Country,
        DateTime CreatedAt,
        bool IsKycApproved,
        bool IsSuspended,
        decimal ConfirmedDeposits,
        decimal PendingDeposits,
        decimal AvailableBalance,
        decimal Invested,
        decimal ReferralEarnings,
        decimal Withdrawn,
        int ActiveInvestments)
    {
        public decimal CurrentBalance => AvailableBalance + Invested + ReferralEarnings;
    }
}
