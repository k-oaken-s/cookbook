namespace DDD.Domain.Orders;

using DDD.Domain.Common;

public class OrderId : ValueObject
{
    public Guid Value { get; }
    
    private OrderId(Guid value)
    {
        Value = value;
    }
    
    public static OrderId Create(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty", nameof(id));
            
        return new OrderId(id);
    }
    
    public static OrderId CreateNew()
    {
        return new OrderId(Guid.NewGuid());
    }
    
    public static implicit operator Guid(OrderId orderId) => orderId.Value;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value.ToString();
}
