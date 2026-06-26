namespace MyFreelance.Application.DTOs.Deposits;

public record DepositNetworkDto(Guid Id, string Name, string Code, string Currency, string WalletAddress, int RequiredConfirmations, decimal MinDeposit);
public record CreateDepositDto(Guid DepositNetworkId, decimal Amount, string? TransactionHash);
public record DepositDto(Guid Id, string NetworkName, decimal Amount, string Status, string? TransactionHash, DateTime CreatedAt, DateTime? ConfirmedAt);
