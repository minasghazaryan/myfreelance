using MyFreelance.Application.DTOs.Deposits;
using MyFreelance.Application.DTOs.Withdrawals;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Application.Interfaces;

public interface IDepositService
{
    Task<IReadOnlyList<DepositNetworkDto>> GetActiveNetworksAsync(CancellationToken cancellationToken = default);
    Task<Deposit> CreateDepositAsync(string userId, CreateDepositDto dto, CancellationToken cancellationToken = default);
    Task ConfirmDepositAsync(Guid depositId, string adminId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepositDto>> GetUserDepositsAsync(string userId, CancellationToken cancellationToken = default);
}
