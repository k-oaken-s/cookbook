namespace DDD.Domain.Customers;

using DDD.Domain.Common;

public class CustomerId : ValueObject
{
    public Guid Value { get; }
    
    private CustomerId(Guid value)
    {
        Value = value;
    }
    
    public static CustomerId Create(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(id));
            
        return new CustomerId(id);
    }
    
    public static CustomerId CreateNew()
    {
        return new CustomerId(Guid.NewGuid());
    }
    
    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value.ToString();
}
