using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Deposits;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Domain.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class DepositService(
    ApplicationDbContext db,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    IReferralService referralService,
    IAuditService auditService) : IDepositService
{
    public async Task<IReadOnlyList<DepositNetworkDto>> GetActiveNetworksAsync(CancellationToken cancellationToken = default)
    {
        return await db.DepositNetworks
            .Where(n => n.IsActive)
            .OrderBy(n => n.SortOrder)
            .Select(n => new DepositNetworkDto(n.Id, n.Name, n.Code, n.Currency, n.WalletAddress, n.RequiredConfirmations, n.MinDeposit))
            .ToListAsync(cancellationToken);
    }

    public async Task<Deposit> CreateDepositAsync(string userId, CreateDepositDto dto, CancellationToken cancellationToken = default)
    {
        var network = await db.DepositNetworks.FindAsync([dto.DepositNetworkId], cancellationToken)
            ?? throw new InvalidOperationException("Deposit network not found.");

        if (dto.Amount < network.MinDeposit)
            throw new InvalidOperationException($"Minimum deposit is {network.MinDeposit} {network.Currency}.");

        var deposit = new Deposit
        {
            UserId = userId,
            DepositNetworkId = dto.DepositNetworkId,
            Amount = dto.Amount,
            TransactionHash = dto.TransactionHash,
            Status = DepositStatus.Pending
        };

        await unitOfWork.Repository<Deposit>().AddAsync(deposit, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(userId, null, AuditAction.Deposit, nameof(Deposit), deposit.Id.ToString(), $"Deposit initiated: {dto.Amount} {network.Currency}", cancellationToken: cancellationToken);
        await notificationService.SendEventNotificationAsync(userId, NotificationEventType.Deposit, cancellationToken: cancellationToken);

        return deposit;
    }

    public async Task ConfirmDepositAsync(Guid depositId, string adminId, CancellationToken cancellationToken = default)
    {
        var deposit = await db.Deposits.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == depositId, cancellationToken)
            ?? throw new InvalidOperationException("Deposit not found.");

        if (deposit.Status == DepositStatus.Confirmed)
            return;

        deposit.Status = DepositStatus.Confirmed;
        deposit.ConfirmedAt = DateTime.UtcNow;
        deposit.ApprovedByAdminId = adminId;

        var wallet = await db.UserWallets.FirstOrDefaultAsync(w => w.UserId == deposit.UserId, cancellationToken);
        if (wallet is null)
        {
            wallet = new UserWallet { UserId = deposit.UserId };
            await db.UserWallets.AddAsync(wallet, cancellationToken);
        }

        wallet.AvailableBalance += deposit.Amount;
        wallet.TotalDeposited += deposit.Amount;

        var transaction = new Transaction
        {
            UserId = deposit.UserId,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Amount = deposit.Amount,
            BalanceAfter = wallet.AvailableBalance,
            Description = "Deposit confirmed",
            ReferenceId = deposit.Id.ToString()
        };
        await db.Transactions.AddAsync(transaction, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await referralService.ProcessReferralCommissionsAsync(deposit.UserId, deposit.Amount, transaction.Id, cancellationToken);
        await auditService.LogAsync(deposit.UserId, adminId, AuditAction.Approve, nameof(Deposit), depositId.ToString(), "Deposit confirmed", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<DepositDto>> GetUserDepositsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await db.Deposits
            .Include(d => d.Network)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DepositDto(d.Id, d.Network.Name, d.Amount, d.Status.ToString(), d.TransactionHash, d.CreatedAt, d.ConfirmedAt))
            .ToListAsync(cancellationToken);
    }
}
