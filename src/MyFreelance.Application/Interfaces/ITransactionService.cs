using MyFreelance.Application.DTOs.Transactions;

namespace MyFreelance.Application.Interfaces;

public interface ITransactionService
{
    Task<IReadOnlyList<TransactionListItemDto>> GetUserTransactionsAsync(
        string userId,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionListItemDto>> GetAllTransactionsAsync(
        int take = 200,
        string? search = null,
        CancellationToken cancellationToken = default);
}
