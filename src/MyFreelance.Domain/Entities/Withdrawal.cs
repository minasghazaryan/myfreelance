using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class Withdrawal : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid DepositNetworkId { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public bool IsImmediate { get; set; }
    public string? RejectionReason { get; set; }
    public string? ProcessedByAdminId { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public DepositNetwork Network { get; set; } = null!;
}
