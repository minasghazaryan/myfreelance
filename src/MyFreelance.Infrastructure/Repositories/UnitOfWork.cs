using System.Collections.Concurrent;
using MyFreelance.Domain.Interfaces;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : class
        => (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(context));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public void Dispose() => context.Dispose();
}
