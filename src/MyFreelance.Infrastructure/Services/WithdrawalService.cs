using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Withdrawals;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Domain.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class WithdrawalService(
    ApplicationDbContext db,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    IAuditService auditService) : IWithdrawalService
{
    private const decimal WithdrawalFeePercent = 1.5m;

    public async Task<decimal> CalculatePenaltyAsync(string userId, decimal amount, bool isImmediate, CancellationToken cancellationToken = default)
    {
        if (!isImmediate) return 0;

        var config = await db.WithdrawalPenaltyConfigs
            .Where(c => c.IsActive && c.AppliesToImmediate)
            .OrderByDescending(c => c.PenaltyPercent)
            .FirstOrDefaultAsync(cancellationToken);

        return config is null ? 0 : amount * (config.PenaltyPercent / 100m);
    }

    public async Task<Withdrawal> RequestWithdrawalAsync(string userId, CreateWithdrawalDto dto, CancellationToken cancellationToken = default)
    {
        var wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Wallet not found.");

        var penalty = await CalculatePenaltyAsync(userId, dto.Amount, dto.IsImmediate, cancellationToken);
        var fee = dto.Amount * (WithdrawalFeePercent / 100m);
        var totalDeduction = dto.Amount + fee + penalty;

        if (wallet.AvailableBalance < totalDeduction)
            throw new InvalidOperationException("Insufficient available balance.");

        wallet.AvailableBalance -= totalDeduction;

        var withdrawal = new Withdrawal
        {
            UserId = userId,
            DepositNetworkId = dto.DepositNetworkId,
            Amount = dto.Amount,
            Fee = fee,
            PenaltyAmount = penalty,
            NetAmount = dto.Amount - fee,
            WalletAddress = dto.WalletAddress,
            IsImmediate = dto.IsImmediate,
            Status = WithdrawalStatus.Pending
        };

        await unitOfWork.Repository<Withdrawal>().AddAsync(withdrawal, cancellationToken);

        await db.Transactions.AddAsync(new Transaction
        {
            UserId = userId,
            Type = TransactionType.Withdrawal,
            Status = TransactionStatus.Pending,
            Amount = -totalDeduction,
            BalanceAfter = wallet.AvailableBalance,
            Description = "Withdrawal requested",
            ReferenceId = withdrawal.Id.ToString()
        }, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await notificationService.SendEventNotificationAsync(userId, NotificationEventType.Withdrawal, cancellationToken: cancellationToken);

        return withdrawal;
    }

    public async Task ApproveWithdrawalAsync(Guid withdrawalId, string adminId, CancellationToken cancellationToken = default)
    {
        var withdrawal = await db.Withdrawals.FindAsync([withdrawalId], cancellationToken)
            ?? throw new InvalidOperationException("Withdrawal not found.");

        withdrawal.Status = WithdrawalStatus.Completed;
        withdrawal.ProcessedByAdminId = adminId;
        withdrawal.ProcessedAt = DateTime.UtcNow;

        var wallet = await db.UserWallets.FirstAsync(w => w.UserId == withdrawal.UserId, cancellationToken);
        wallet.TotalWithdrawn += withdrawal.Amount;

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(withdrawal.UserId, adminId, AuditAction.Approve, nameof(Withdrawal), withdrawalId.ToString(), "Withdrawal approved", cancellationToken: cancellationToken);
    }

    public async Task RejectWithdrawalAsync(Guid withdrawalId, string adminId, string reason, CancellationToken cancellationToken = default)
    {
        var withdrawal = await db.Withdrawals.FindAsync([withdrawalId], cancellationToken)
            ?? throw new InvalidOperationException("Withdrawal not found.");

        withdrawal.Status = WithdrawalStatus.Rejected;
        withdrawal.RejectionReason = reason;
        withdrawal.ProcessedByAdminId = adminId;
        withdrawal.ProcessedAt = DateTime.UtcNow;

        var wallet = await db.UserWallets.FirstAsync(w => w.UserId == withdrawal.UserId, cancellationToken);
        wallet.AvailableBalance += withdrawal.Amount + withdrawal.Fee + withdrawal.PenaltyAmount;

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(withdrawal.UserId, adminId, AuditAction.Reject, nameof(Withdrawal), withdrawalId.ToString(), $"Withdrawal rejected: {reason}", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<WithdrawalDto>> GetUserWithdrawalsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await db.Withdrawals
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WithdrawalDto(w.Id, w.Amount, w.Fee, w.PenaltyAmount, w.NetAmount, w.Status.ToString(), w.WalletAddress, w.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
