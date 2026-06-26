using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class UserWallet : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public decimal InvestedCapital { get; set; }
    public decimal ProjectedEarnings { get; set; }
    public decimal ReferralEarnings { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal TotalWithdrawn { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public decimal CurrentBalance => AvailableBalance + InvestedCapital + ReferralEarnings;
}
