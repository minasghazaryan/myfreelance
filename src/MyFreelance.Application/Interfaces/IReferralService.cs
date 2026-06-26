using MyFreelance.Application.DTOs.Referrals;

namespace MyFreelance.Application.Interfaces;

public interface IReferralService
{
    Task<string> GenerateReferralCodeAsync(string userId, CancellationToken cancellationToken = default);
    Task ProcessReferralCommissionsAsync(string sourceUserId, decimal sourceAmount, Guid? transactionId, CancellationToken cancellationToken = default);
    Task<ReferralTreeDto> GetReferralTreeAsync(string userId, CancellationToken cancellationToken = default);
    Task<ReferralStatsDto> GetReferralStatsAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReferralConfigDto>> GetReferralConfigAsync(CancellationToken cancellationToken = default);
}
