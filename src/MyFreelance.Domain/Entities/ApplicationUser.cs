using Microsoft.AspNetCore.Identity;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ReferralCode { get; set; }
    public string? ReferredByUserId { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsKycApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string CountryCode { get; set; } = "GH";

    public ApplicationUser? ReferredBy { get; set; }
    public ICollection<ApplicationUser> Referrals { get; set; } = [];
    public KycProfile? KycProfile { get; set; }
    public UserWallet? Wallet { get; set; }
    public ICollection<Investment> Investments { get; set; } = [];
    public ICollection<Deposit> Deposits { get; set; } = [];
    public ICollection<Withdrawal> Withdrawals { get; set; } = [];
    public ICollection<ReferralCommission> ReferralCommissions { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];

    public string FullName => $"{FirstName} {LastName}".Trim();
}
