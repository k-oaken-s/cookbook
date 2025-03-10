namespace DDD.Domain.Orders.Events;

using DDD.Domain.Common;
using DDD.Domain.Customers;

public class OrderCreatedEvent : IDomainEvent
{
    public OrderId OrderId { get; }
    public CustomerId CustomerId { get; }
    public DateTime OccurredOn { get; }
    
    public OrderCreatedEvent(OrderId orderId, CustomerId customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
        OccurredOn = DateTime.UtcNow;
    }
}