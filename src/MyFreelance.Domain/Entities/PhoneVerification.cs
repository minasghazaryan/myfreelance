using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class PhoneVerification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string Provider { get; set; } = "Twilio";
    public DateTime ExpiresAt { get; set; }
    public bool IsVerified { get; set; }
    public int AttemptCount { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
