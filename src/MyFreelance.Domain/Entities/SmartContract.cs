using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class SmartContract : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public SmartContractStatus Status { get; set; } = SmartContractStatus.Active;
    public decimal TotalAllocatedCapital { get; set; }
    public string? Description { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    public ICollection<SmartContractStrategy> Strategies { get; set; } = [];
}
