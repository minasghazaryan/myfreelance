namespace MyFreelance.Application.DTOs.Withdrawals;

public record CreateWithdrawalDto(Guid DepositNetworkId, decimal Amount, string WalletAddress, bool IsImmediate);
public record WithdrawalDto(Guid Id, decimal Amount, decimal Fee, decimal PenaltyAmount, decimal NetAmount, string Status, string WalletAddress, DateTime CreatedAt);
