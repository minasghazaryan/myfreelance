using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Kyc;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Domain.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class KycService(
    ApplicationDbContext db,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    IAuditService auditService) : IKycService
{
    public async Task<KycProfile?> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
        => await db.KycProfiles.Include(k => k.Documents).FirstOrDefaultAsync(k => k.UserId == userId, cancellationToken);

    public async Task<KycDetailDto?> GetProfileByIdAsync(Guid kycId, CancellationToken cancellationToken = default)
    {
        var profile = await db.KycProfiles
            .Include(k => k.Documents)
            .FirstOrDefaultAsync(k => k.Id == kycId, cancellationToken);

        if (profile is null) return null;

        return new KycDetailDto(
            profile.Id,
            profile.UserId,
            profile.FirstName,
            profile.LastName,
            profile.DateOfBirth,
            profile.Gender.ToString(),
            profile.Country,
            profile.Nationality,
            profile.Address,
            profile.City,
            profile.PostalCode,
            profile.Email,
            profile.MobileNumber,
            profile.Status.ToString(),
            profile.RejectionReason,
            profile.CreatedAt,
            profile.UpdatedAt,
            profile.Documents
                .OrderBy(d => d.DocumentType)
                .Select(d => new KycDocumentDto(d.Id, d.DocumentType.ToString(), d.FileName, d.ContentType, d.FileSizeBytes, d.CreatedAt))
                .ToList());
    }

    public async Task SaveDocumentAsync(Guid kycProfileId, DocumentType type, string fileName, string fileContentBase64, string contentType, long fileSizeBytes, CancellationToken cancellationToken = default)
    {
        var existing = await db.KycDocuments
            .FirstOrDefaultAsync(d => d.KycProfileId == kycProfileId && d.DocumentType == type, cancellationToken);

        if (existing is not null)
        {
            existing.FileName = fileName;
            existing.FileContentBase64 = fileContentBase64;
            existing.ContentType = contentType;
            existing.FileSizeBytes = fileSizeBytes;
            existing.UpdatedAt = DateTime.UtcNow;
            unitOfWork.Repository<KycDocument>().Update(existing);
        }
        else
        {
            await unitOfWork.Repository<KycDocument>().AddAsync(new KycDocument
            {
                KycProfileId = kycProfileId,
                DocumentType = type,
                FileName = fileName,
                FileContentBase64 = fileContentBase64,
                ContentType = contentType,
                FileSizeBytes = fileSizeBytes
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<KycProfile> SubmitKycAsync(string userId, SubmitKycDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await db.KycProfiles.FirstOrDefaultAsync(k => k.UserId == userId, cancellationToken);
        if (existing is not null && existing.Status is KycStatus.Approved or KycStatus.UnderReview)
            throw new InvalidOperationException("KYC already submitted or approved.");

        Enum.TryParse<Gender>(dto.Gender, true, out var gender);

        var profile = existing ?? new KycProfile { UserId = userId };
        profile.FirstName = dto.FirstName;
        profile.LastName = dto.LastName;
        profile.DateOfBirth = dto.DateOfBirth;
        profile.Gender = gender;
        profile.Country = dto.Country;
        profile.Nationality = dto.Nationality;
        profile.Address = dto.Address;
        profile.City = dto.City;
        profile.PostalCode = dto.PostalCode;
        profile.Email = dto.Email;
        profile.MobileNumber = dto.MobileNumber;
        profile.Status = KycStatus.Pending;
        profile.UpdatedAt = DateTime.UtcNow;

        if (existing is null)
            await unitOfWork.Repository<KycProfile>().AddAsync(profile, cancellationToken);
        else
            unitOfWork.Repository<KycProfile>().Update(profile);

        await db.SaveChangesAsync(cancellationToken);
        await notificationService.SendEventNotificationAsync(
            userId,
            NotificationEventType.Verification,
            new Dictionary<string, string>
            {
                ["Status"] = "Pending Review",
                ["Description"] = "Your KYC documents were submitted successfully and are awaiting compliance review."
            },
            cancellationToken);

        return profile;
    }

    public async Task UpdateStatusAsync(Guid kycId, KycStatus status, string adminId, string? rejectionReason = null, CancellationToken cancellationToken = default)
    {
        var profile = await db.KycProfiles.Include(k => k.User).FirstOrDefaultAsync(k => k.Id == kycId, cancellationToken)
            ?? throw new InvalidOperationException("KYC profile not found.");

        profile.Status = status;
        profile.RejectionReason = rejectionReason;
        profile.ReviewedByAdminId = adminId;
        profile.ReviewedAt = DateTime.UtcNow;

        if (status == KycStatus.Approved)
        {
            profile.User.IsKycApproved = true;
        }

        await db.SaveChangesAsync(cancellationToken);

        var statusDescription = status switch
        {
            KycStatus.Approved => "Your identity verification has been approved. You can now invest and withdraw.",
            KycStatus.Rejected => "Your KYC application was rejected. Please review the reason and resubmit if needed.",
            KycStatus.UnderReview => "Your KYC application is currently under review by our compliance team.",
            KycStatus.Pending => "Your KYC application is pending review.",
            _ => $"Your KYC status changed to {status}."
        };

        await notificationService.SendEventNotificationAsync(
            profile.UserId,
            NotificationEventType.KycStatusChange,
            new Dictionary<string, string>
            {
                ["Status"] = status.ToString(),
                ["Description"] = statusDescription,
                ["RejectionReason"] = rejectionReason ?? "—"
            },
            cancellationToken);
        await auditService.LogAsync(profile.UserId, adminId, status == KycStatus.Approved ? Domain.Enums.AuditAction.Approve : Domain.Enums.AuditAction.Reject, nameof(KycProfile), kycId.ToString(), $"KYC status changed to {status}", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<KycProfileDto>> GetPendingReviewsAsync(CancellationToken cancellationToken = default)
    {
        return await db.KycProfiles
            .Where(k => k.Status == KycStatus.Pending || k.Status == KycStatus.UnderReview)
            .OrderBy(k => k.CreatedAt)
            .Select(k => new KycProfileDto(k.Id, k.UserId, k.FirstName + " " + k.LastName, k.Status.ToString(), k.CreatedAt, k.RejectionReason, k.Documents.Count))
            .ToListAsync(cancellationToken);
    }
}
