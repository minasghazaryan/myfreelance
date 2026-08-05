using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class YieldAccrualService(
    ApplicationDbContext db,
    INotificationService notificationService,
    IAuditService auditService) : IYieldAccrualService
{
    public async Task ProcessDueAccrualsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var investmentIds = await db.Investments
            .AsNoTracking()
            .Where(i => i.Status == InvestmentStatus.Active)
            .Where(i => i.AccrualDaysCompleted < InvestmentConstants.PlanDurationDays)
            .Where(i => i.LastAccrualDate == null || i.LastAccrualDate < today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        foreach (var investmentId in investmentIds)
        {
            await AccrueInvestmentForDateAsync(investmentId, today, cancellationToken);
        }
    }

    public async Task AccrueInvestmentForDateAsync(Guid investmentId, DateOnly accrualDate, CancellationToken cancellationToken = default)
    {
        var investment = await db.Investments
            .Include(i => i.User)
            .Include(i => i.Tier)
            .FirstOrDefaultAsync(i => i.Id == investmentId, cancellationToken);

        if (investment is null
            || investment.Status != InvestmentStatus.Active
            || investment.User.IsSuspended
            || investment.AccrualDaysCompleted >= InvestmentConstants.PlanDurationDays)
        {
            return;
        }

        var accrualDateUtc = accrualDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        if (investment.LastAccrualDate.HasValue
            && DateOnly.FromDateTime(investment.LastAccrualDate.Value) >= accrualDate)
        {
            return;
        }

        var wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == investment.UserId, cancellationToken);
        if (wallet is null)
            return;

        var dailyYield = InvestmentConstants.CalculateDailyYield(investment.Amount, investment.ProjectedYieldPercent);
        if (dailyYield <= 0)
            return;

        wallet.AvailableBalance += dailyYield;
        wallet.ProjectedEarnings += dailyYield;
        investment.AccruedAmount += dailyYield;
        investment.AccrualDaysCompleted += 1;
        investment.LastAccrualDate = accrualDateUtc;

        await db.Transactions.AddAsync(new Transaction
        {
            UserId = investment.UserId,
            Type = TransactionType.YieldCredit,
            Status = TransactionStatus.Completed,
            Amount = dailyYield,
            BalanceAfter = wallet.AvailableBalance,
            Description = $"Daily yield from {investment.Tier.Name} plan (day {investment.AccrualDaysCompleted}/{InvestmentConstants.PlanDurationDays})",
            ReferenceId = investment.Id.ToString()
        }, cancellationToken);

        if (investment.AccrualDaysCompleted >= InvestmentConstants.PlanDurationDays)
            await AutoReinvestAsync(investment, wallet, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task AutoReinvestAsync(Investment investment, UserWallet wallet, CancellationToken cancellationToken)
    {
        investment.Status = InvestmentStatus.Matured;
        investment.MaturedAt = DateTime.UtcNow;

        var tier = await db.InvestmentTiers.FindAsync([investment.InvestmentTierId], cancellationToken);
        var yieldPercent = tier?.ProjectedYieldPercent ?? investment.ProjectedYieldPercent;
        var tierName = tier?.Name ?? investment.Tier.Name;

        var reinvested = new Investment
        {
            UserId = investment.UserId,
            InvestmentTierId = investment.InvestmentTierId,
            Amount = investment.Amount,
            ProjectedYieldPercent = yieldPercent,
            Status = InvestmentStatus.Active,
            LastAccrualDate = investment.LastAccrualDate
        };

        await db.Investments.AddAsync(reinvested, cancellationToken);

        await auditService.LogAsync(
            investment.UserId,
            null,
            AuditAction.Create,
            nameof(Investment),
            reinvested.Id.ToString(),
            $"Investment auto-reinvested in {tierName} for ${investment.Amount:N2}",
            cancellationToken: cancellationToken);

        await notificationService.SendEventNotificationAsync(
            investment.UserId,
            NotificationEventType.TierUpgrade,
            new Dictionary<string, string>
            {
                ["TierName"] = tierName,
                ["Amount"] = $"${investment.Amount:N2}",
                ["ProjectedYield"] = $"{yieldPercent:N1}%",
                ["Status"] = "Reinvested",
                ["Description"] = $"Your {tierName} plan completed 30 days and was automatically reinvested for another cycle."
            },
            cancellationToken);
    }
}
