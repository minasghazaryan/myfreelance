using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class Deposit : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid DepositNetworkId { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionHash { get; set; }
    public DepositStatus Status { get; set; } = DepositStatus.Pending;
    public int Confirmations { get; set; }
    public string? AdminNotes { get; set; }
    public string? ApprovedByAdminId { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public DepositNetwork Network { get; set; } = null!;
}
