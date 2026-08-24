using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Investments;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Domain.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class InvestmentService(
    ApplicationDbContext db,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    IAuditService auditService,
    IYieldAccrualService yieldAccrualService) : IInvestmentService
{
    public async Task<IReadOnlyList<InvestmentTierDto>> GetActiveTiersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await db.InvestmentTiers
            .Where(t => t.IsActive)
            .Where(t => t.PromoEndUtc == null || t.PromoEndUtc >= now)
            .OrderBy(t => t.SortOrder)
            .Select(t => new InvestmentTierDto(
                t.Id,
                t.Name,
                t.Description,
                t.PackageDetails,
                t.RiskLevel.ToString(),
                t.ProjectedYieldPercent,
                t.MinInvestment,
                t.MaxInvestment,
                t.AccentColor,
                t.InsuranceNotice,
                t.ImagePath,
                t.PromoBannerText,
                t.PromoEndUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<Investment> CreateInvestmentAsync(string userId, CreateInvestmentDto dto, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.IsKycApproved)
            throw new InvalidOperationException("KYC approval required before investing.");

        var tier = await db.InvestmentTiers.FindAsync([dto.InvestmentTierId], cancellationToken)
            ?? throw new InvalidOperationException("Investment tier not found.");

        if (dto.Amount < tier.MinInvestment || dto.Amount > tier.MaxInvestment)
            throw new InvalidOperationException($"Investment amount must be between {tier.MinInvestment} and {tier.MaxInvestment}.");

        var wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Wallet not found.");

        if (wallet.AvailableBalance < dto.Amount)
            throw new InvalidOperationException("Insufficient available balance.");

        wallet.AvailableBalance -= dto.Amount;
        wallet.InvestedCapital += dto.Amount;

        var investment = new Investment
        {
            UserId = userId,
            InvestmentTierId = dto.InvestmentTierId,
            Amount = dto.Amount,
            ProjectedYieldPercent = tier.ProjectedYieldPercent,
            Status = InvestmentStatus.Active
        };

        await unitOfWork.Repository<Investment>().AddAsync(investment, cancellationToken);

        await db.Transactions.AddAsync(new Transaction
        {
            UserId = userId,
            Type = TransactionType.Investment,
            Status = TransactionStatus.Completed,
            Amount = -dto.Amount,
            BalanceAfter = wallet.AvailableBalance,
            Description = $"Investment in {tier.Name} tier",
            ReferenceId = investment.Id.ToString()
        }, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await yieldAccrualService.AccrueInvestmentForDateAsync(
            investment.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            cancellationToken);
        await notificationService.SendEventNotificationAsync(
            userId,
            NotificationEventType.TierUpgrade,
            new Dictionary<string, string>
            {
                ["TierName"] = tier.Name,
                ["Amount"] = $"${dto.Amount:N2}",
                ["ProjectedYield"] = $"{tier.ProjectedYieldPercent:N1}%",
                ["Status"] = "Active",
                ["Description"] = $"Your investment in the {tier.Name} tier is now active."
            },
            cancellationToken);
        await auditService.LogAsync(userId, null, AuditAction.Create, nameof(Investment), investment.Id.ToString(), $"Investment created in {tier.Name}", cancellationToken: cancellationToken);

        return investment;
    }

    public async Task<IReadOnlyList<InvestmentDto>> GetUserInvestmentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await db.Investments
            .Include(i => i.Tier)
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvestmentDto(
                i.Id,
                i.Tier.Name,
                i.Amount,
                i.ProjectedYieldPercent,
                i.Status.ToString(),
                i.CreatedAt,
                i.AccrualDaysCompleted,
                i.AccruedAmount))
            .ToListAsync(cancellationToken);
    }
}
