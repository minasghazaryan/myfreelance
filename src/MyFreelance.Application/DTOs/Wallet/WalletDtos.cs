namespace MyFreelance.Application.DTOs.Wallet;

public record BonusAwardDto(
    Guid TransactionId,
    string UserEmail,
    string UserName,
    decimal Amount,
    string Description,
    DateTime CreatedAt);

public record InvestorOptionDto(string Id, string Name, string Email);
