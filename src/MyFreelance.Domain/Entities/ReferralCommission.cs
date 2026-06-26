using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class ReferralCommission : BaseEntity
{
    public string BeneficiaryUserId { get; set; } = string.Empty;
    public string SourceUserId { get; set; } = string.Empty;
    public int Level { get; set; }
    public decimal Percentage { get; set; }
    public decimal SourceAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public Guid? SourceTransactionId { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }

    public ApplicationUser BeneficiaryUser { get; set; } = null!;
    public ApplicationUser SourceUser { get; set; } = null!;
}
