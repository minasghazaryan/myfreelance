using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Transactions;
using MyFreelance.Application.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class TransactionService(ApplicationDbContext db) : ITransactionService
{
    public async Task<IReadOnlyList<TransactionListItemDto>> GetUserTransactionsAsync(
        string userId,
        int take = 100,
        CancellationToken cancellationToken = default)
        => await db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .Select(t => new TransactionListItemDto(
                t.Id,
                null,
                null,
                t.Type.ToString(),
                t.Status.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TransactionListItemDto>> GetAllTransactionsAsync(
        int take = 200,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Transactions
            .Include(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                t.User.Email!.Contains(term)
                || t.User.FirstName.Contains(term)
                || t.User.LastName.Contains(term)
                || t.Description.Contains(term)
                || t.Type.ToString().Contains(term));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .Select(t => new TransactionListItemDto(
                t.Id,
                t.User.FirstName + " " + t.User.LastName,
                t.User.Email,
                t.Type.ToString(),
                t.Status.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
