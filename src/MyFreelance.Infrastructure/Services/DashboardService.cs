using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Dashboard;
using MyFreelance.Application.DTOs.SmartContracts;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class DashboardService(ApplicationDbContext db) : IDashboardService
{
    public async Task<PortfolioOverviewDto> GetPortfolioOverviewAsync(string userId, CancellationToken cancellationToken = default)
    {
        var wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        if (wallet is null)
            return new PortfolioOverviewDto(0, 0, 0, 0, 0);

        return new PortfolioOverviewDto(wallet.CurrentBalance, wallet.InvestedCapital, wallet.ProjectedEarnings, wallet.ReferralEarnings, wallet.AvailableBalance);
    }

    public async Task<PortfolioAnalyticsDto> GetPortfolioAnalyticsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var transactions = await db.Transactions
            .Where(t => t.UserId == userId && t.Status == TransactionStatus.Completed)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var months = Enumerable.Range(0, 6).Select(i => DateTime.UtcNow.AddMonths(-5 + i).ToString("MMM yyyy")).ToList();
        decimal running = 0;

        var growth = months.Select(m =>
        {
            var monthTx = transactions.Where(t => t.CreatedAt.ToString("MMM yyyy") == m);
            running += monthTx.Sum(t => t.Amount);
            return new ChartDataPoint(m, Math.Max(0, running));
        }).ToList();

        return new PortfolioAnalyticsDto(
            growth,
            months.Select(m => new ChartDataPoint(m, transactions.Where(t => t.Type == TransactionType.YieldCredit && t.CreatedAt.ToString("MMM yyyy") == m).Sum(t => t.Amount))).ToList(),
            months.Select(m => new ChartDataPoint(m, transactions.Where(t => t.Type == TransactionType.Deposit && t.CreatedAt.ToString("MMM yyyy") == m).Sum(t => t.Amount))).ToList(),
            months.Select(m => new ChartDataPoint(m, Math.Abs(transactions.Where(t => t.Type == TransactionType.Withdrawal && t.CreatedAt.ToString("MMM yyyy") == m).Sum(t => t.Amount)))).ToList()
        );
    }

    public async Task<IReadOnlyList<ActivityFeedItemDto>> GetActivityFeedAsync(string userId, int take = 20, CancellationToken cancellationToken = default)
    {
        return await db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .Select(t => new ActivityFeedItemDto(t.Type.ToString(), t.Description, t.Amount, t.CreatedAt, t.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<SmartContractDashboardDto> GetSmartContractDashboardAsync(CancellationToken cancellationToken = default)
    {
        var contracts = await db.SmartContracts.Include(c => c.Strategies).Where(c => c.Status == SmartContractStatus.Active).ToListAsync(cancellationToken);
        var strategies = contracts.SelectMany(c => c.Strategies.Where(s => s.IsActive)).ToList();

        return new SmartContractDashboardDto(
            contracts.Sum(c => c.TotalAllocatedCapital),
            strategies.Count,
            strategies.Select(s => new StrategyDistributionDto(s.Name, s.AllocationPercent, s.HistoricalReturnPercent, s.RiskScore)).ToList(),
            Enumerable.Range(0, 12).Select(i => new PerformanceDataPoint(DateTime.UtcNow.AddMonths(-11 + i).ToString("MMM"), (decimal)(Random.Shared.NextDouble() * 15 + 5))).ToList()
        );
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        return new AdminDashboardDto(
            await db.Users.CountAsync(cancellationToken),
            await db.Deposits.Where(d => d.Status == DepositStatus.Confirmed).SumAsync(d => d.Amount, cancellationToken),
            await db.Withdrawals.Where(w => w.Status == WithdrawalStatus.Completed).SumAsync(w => w.Amount, cancellationToken),
            await db.KycProfiles.CountAsync(k => k.Status == KycStatus.Pending || k.Status == KycStatus.UnderReview, cancellationToken),
            await db.Deposits.CountAsync(d => d.Status == DepositStatus.Pending, cancellationToken),
            await db.Withdrawals.CountAsync(w => w.Status == WithdrawalStatus.Pending, cancellationToken),
            await db.Deposits.Where(d => d.Status == DepositStatus.Confirmed).SumAsync(d => d.Amount, cancellationToken) * 0.02m
        );
    }
}
