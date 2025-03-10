namespace DDD.Infrastructure.DomainEvents;

using DDD.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

public interface IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}