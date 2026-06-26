using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class WithdrawalPenaltyConfig : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal PenaltyPercent { get; set; }
    public int MinDaysHeld { get; set; }
    public bool AppliesToImmediate { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
