namespace MyFreelance.Application.DTOs.Transactions;

public record TransactionListItemDto(
    Guid Id,
    string? UserName,
    string? UserEmail,
    string Type,
    string Status,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTime CreatedAt);
