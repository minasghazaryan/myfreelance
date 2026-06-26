using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class ReferralConfig : BaseEntity
{
    public int Level { get; set; }
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}
