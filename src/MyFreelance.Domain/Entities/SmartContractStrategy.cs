using MyFreelance.Domain.Common;

namespace MyFreelance.Domain.Entities;

public class SmartContractStrategy : BaseEntity
{
    public Guid SmartContractId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal AllocationPercent { get; set; }
    public decimal HistoricalReturnPercent { get; set; }
    public decimal RiskScore { get; set; }
    public bool IsActive { get; set; } = true;

    public SmartContract SmartContract { get; set; } = null!;
}
