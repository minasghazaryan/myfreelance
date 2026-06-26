using MyFreelance.Application.DTOs.Withdrawals;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Application.Interfaces;

public interface IWithdrawalService
{
    Task<Withdrawal> RequestWithdrawalAsync(string userId, CreateWithdrawalDto dto, CancellationToken cancellationToken = default);
    Task ApproveWithdrawalAsync(Guid withdrawalId, string adminId, CancellationToken cancellationToken = default);
    Task RejectWithdrawalAsync(Guid withdrawalId, string adminId, string reason, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WithdrawalDto>> GetUserWithdrawalsAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> CalculatePenaltyAsync(string userId, decimal amount, bool isImmediate, CancellationToken cancellationToken = default);
}
