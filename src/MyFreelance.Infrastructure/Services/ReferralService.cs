using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Referrals;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class ReferralService(ApplicationDbContext db, INotificationService notificationService) : IReferralService
{
    public async Task<string> GenerateReferralCodeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (!string.IsNullOrEmpty(user.ReferralCode))
            return user.ReferralCode;

        user.ReferralCode = $"AW{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        await db.SaveChangesAsync(cancellationToken);
        return user.ReferralCode;
    }

    public async Task ProcessReferralCommissionsAsync(string sourceUserId, decimal sourceAmount, Guid? transactionId, CancellationToken cancellationToken = default)
    {
        var configs = await db.ReferralConfigs.Where(c => c.IsActive).OrderBy(c => c.Level).ToListAsync(cancellationToken);
        if (configs.Count == 0) return;

        var currentUserId = sourceUserId;
        foreach (var config in configs)
        {
            var user = await db.Users.FindAsync([currentUserId], cancellationToken);
            if (user?.ReferredByUserId is null) break;

            var commissionAmount = sourceAmount * (config.Percentage / 100m);
            var commission = new ReferralCommission
            {
                BeneficiaryUserId = user.ReferredByUserId,
                SourceUserId = sourceUserId,
                Level = config.Level,
                Percentage = config.Percentage,
                SourceAmount = sourceAmount,
                CommissionAmount = commissionAmount,
                SourceTransactionId = transactionId,
                IsPaid = true,
                PaidAt = DateTime.UtcNow
            };
            await db.ReferralCommissions.AddAsync(commission, cancellationToken);

            var wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.ReferredByUserId, cancellationToken);
            if (wallet is not null)
            {
                wallet.ReferralEarnings += commissionAmount;
                wallet.AvailableBalance += commissionAmount;
            }

            await notificationService.SendEventNotificationAsync(user.ReferredByUserId, Domain.Enums.NotificationEventType.ReferralReward, cancellationToken: cancellationToken);
            currentUserId = user.ReferredByUserId;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ReferralStatsDto> GetReferralStatsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var level1 = await db.Users.CountAsync(u => u.ReferredByUserId == userId, cancellationToken);
        var level1Ids = await db.Users.Where(u => u.ReferredByUserId == userId).Select(u => u.Id).ToListAsync(cancellationToken);
        var level2 = level1Ids.Count > 0 ? await db.Users.CountAsync(u => level1Ids.Contains(u.ReferredByUserId!), cancellationToken) : 0;
        var level2Ids = level1Ids.Count > 0 ? await db.Users.Where(u => level1Ids.Contains(u.ReferredByUserId!)).Select(u => u.Id).ToListAsync(cancellationToken) : [];
        var level3 = level2Ids.Count > 0 ? await db.Users.CountAsync(u => level2Ids.Contains(u.ReferredByUserId!), cancellationToken) : 0;

        var totalRewards = await db.ReferralCommissions
            .Where(c => c.BeneficiaryUserId == userId)
            .SumAsync(c => c.CommissionAmount, cancellationToken);

        return new ReferralStatsDto(level1 + level2 + level3, totalRewards, level1, level2, level3);
    }

    public async Task<ReferralTreeDto> GetReferralTreeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var code = await GenerateReferralCodeAsync(userId, cancellationToken);
        var stats = await GetReferralStatsAsync(userId, cancellationToken);
        var directReferrals = await db.Users.Where(u => u.ReferredByUserId == userId).ToListAsync(cancellationToken);

        var tree = new List<ReferralTreeNodeDto>();
        foreach (var referral in directReferrals)
        {
            var childStats = await GetReferralStatsAsync(referral.Id, cancellationToken);
            tree.Add(new ReferralTreeNodeDto(referral.Id, referral.FullName, 1, childStats.Level1Count, childStats.TotalRewards, []));
        }

        return new ReferralTreeDto(code, $"/Account/Register?ref={code}", stats, tree);
    }

    public async Task<IReadOnlyList<ReferralConfigDto>> GetReferralConfigAsync(CancellationToken cancellationToken = default)
    {
        return await db.ReferralConfigs
            .Where(c => c.IsActive)
            .OrderBy(c => c.Level)
            .Select(c => new ReferralConfigDto(c.Level, c.Percentage, c.Description))
            .ToListAsync(cancellationToken);
    }
}
