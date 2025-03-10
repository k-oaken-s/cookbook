namespace DDD.Domain.Orders.Events;

using DDD.Domain.Common;

public class OrderCancelledEvent : IDomainEvent
{
    public OrderId OrderId { get; }
    public DateTime OccurredOn { get; }
    
    public OrderCancelledEvent(OrderId orderId)
    {
        OrderId = orderId;
        OccurredOn = DateTime.UtcNow;
    }
}
