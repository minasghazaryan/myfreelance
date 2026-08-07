using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Wallet;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class WalletService(
    ApplicationDbContext db,
    IAuditService auditService,
    INotificationService notificationService) : IWalletService
{
    public async Task AwardBonusAsync(
        string userId,
        decimal amount,
        string description,
        string adminId,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Bonus amount must be greater than zero.");

        var trimmedDescription = description.Trim();
        if (string.IsNullOrWhiteSpace(trimmedDescription))
            throw new InvalidOperationException("Bonus description is required.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        var wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        if (wallet is null)
        {
            wallet = new UserWallet { UserId = userId };
            await db.UserWallets.AddAsync(wallet, cancellationToken);
        }

        wallet.AvailableBalance += amount;

        var transaction = new Transaction
        {
            UserId = userId,
            Type = TransactionType.Bonus,
            Status = TransactionStatus.Completed,
            Amount = amount,
            BalanceAfter = wallet.AvailableBalance,
            Description = trimmedDescription,
            ReferenceId = adminId
        };

        await db.Transactions.AddAsync(transaction, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            userId,
            adminId,
            AuditAction.Create,
            nameof(Transaction),
            transaction.Id.ToString(),
            $"Bonus awarded: ${amount:N2} — {trimmedDescription}",
            cancellationToken: cancellationToken);

        await notificationService.SendEventNotificationAsync(
            userId,
            NotificationEventType.BonusAward,
            new Dictionary<string, string>
            {
                ["Amount"] = $"${amount:N2}",
                ["Description"] = trimmedDescription,
                ["Status"] = "Credited"
            },
            cancellationToken);
    }

    public async Task<IReadOnlyList<BonusAwardDto>> GetRecentBonusesAsync(int take = 50, CancellationToken cancellationToken = default)
        => await db.Transactions
            .Include(t => t.User)
            .Where(t => t.Type == TransactionType.Bonus)
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .Select(t => new BonusAwardDto(
                t.Id,
                t.User.Email!,
                t.User.FirstName + " " + t.User.LastName,
                t.Amount,
                t.Description,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<InvestorOptionDto>> GetInvestorOptionsAsync(CancellationToken cancellationToken = default)
    {
        var investorRoleId = await db.Roles
            .Where(r => r.Name == AppRoles.Investor)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (investorRoleId is null)
            return [];

        return await db.UserRoles
            .Where(ur => ur.RoleId == investorRoleId)
            .Join(db.Users, ur => ur.UserId, u => u.Id, (_, u) => u)
            .Where(u => !u.IsSuspended)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new InvestorOptionDto(u.Id, u.FirstName + " " + u.LastName, u.Email!))
            .ToListAsync(cancellationToken);
    }
}
