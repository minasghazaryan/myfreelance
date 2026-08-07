using MyFreelance.Application.DTOs.Wallet;

namespace MyFreelance.Application.Interfaces;

public interface IWalletService
{
    Task AwardBonusAsync(string userId, decimal amount, string description, string adminId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BonusAwardDto>> GetRecentBonusesAsync(int take = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestorOptionDto>> GetInvestorOptionsAsync(CancellationToken cancellationToken = default);
}
