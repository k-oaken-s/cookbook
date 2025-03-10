namespace DDD.Domain.Products.Events;

using DDD.Domain.Common;

public class ProductOutOfStockEvent : IDomainEvent
{
    public ProductId ProductId { get; }
    public DateTime OccurredOn { get; }
    
    public ProductOutOfStockEvent(ProductId productId)
    {
        ProductId = productId;
        OccurredOn = DateTime.UtcNow;
    }
}