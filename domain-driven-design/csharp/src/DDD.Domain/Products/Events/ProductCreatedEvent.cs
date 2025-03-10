namespace DDD.Domain.Products.Events;

using DDD.Domain.Common;

public class ProductCreatedEvent : IDomainEvent
{
    public ProductId ProductId { get; }
    public DateTime OccurredOn { get; }
    
    public ProductCreatedEvent(ProductId productId)
    {
        ProductId = productId;
        OccurredOn = DateTime.UtcNow;
    }
}