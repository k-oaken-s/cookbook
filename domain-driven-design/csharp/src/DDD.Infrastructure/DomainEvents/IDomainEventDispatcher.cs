namespace DDD.Infrastructure.DomainEvents;

using DDD.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}