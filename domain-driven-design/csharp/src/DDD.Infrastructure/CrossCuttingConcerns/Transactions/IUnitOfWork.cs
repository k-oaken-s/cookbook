namespace DDD.Infrastructure.CrossCuttingConcerns.Transactions;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T?> ExecuteWithTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default);
}