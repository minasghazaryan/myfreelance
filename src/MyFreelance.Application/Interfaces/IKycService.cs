using MyFreelance.Application.DTOs.Kyc;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Application.Interfaces;

public interface IKycService
{
    Task<KycProfile?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<KycProfile> SubmitKycAsync(string userId, SubmitKycDto dto, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid kycId, KycStatus status, string adminId, string? rejectionReason = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KycProfileDto>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);
}
