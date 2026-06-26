using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class DepositNetwork : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Currency { get; set; } = "USDT";
    public string WalletAddress { get; set; } = string.Empty;
    public int RequiredConfirmations { get; set; } = 12;
    public decimal MinDeposit { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Deposit> Deposits { get; set; } = [];
}
