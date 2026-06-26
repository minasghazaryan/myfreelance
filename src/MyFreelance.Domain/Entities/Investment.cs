using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class Investment : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid InvestmentTierId { get; set; }
    public decimal Amount { get; set; }
    public decimal ProjectedYieldPercent { get; set; }
    public InvestmentStatus Status { get; set; } = InvestmentStatus.Active;
    public DateTime? MaturedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public InvestmentTier Tier { get; set; } = null!;
}
