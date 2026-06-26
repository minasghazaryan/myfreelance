using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class InvestmentTier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public decimal ProjectedYieldPercent { get; set; }
    public decimal MinInvestment { get; set; }
    public decimal MaxInvestment { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? IconClass { get; set; }
    public string? AccentColor { get; set; }

    public ICollection<Investment> Investments { get; set; } = [];
}
