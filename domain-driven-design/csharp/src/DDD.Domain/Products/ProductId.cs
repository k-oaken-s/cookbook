namespace DDD.Domain.Products;

using DDD.Domain.Common;

public class ProductId : ValueObject
{
    public Guid Value { get; }
    
    private ProductId(Guid value)
    {
        Value = value;
    }
    
    public static ProductId Create(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(id));
            
        return new ProductId(id);
    }
    
    public static ProductId CreateNew()
    {
        return new ProductId(Guid.NewGuid());
    }
    
    public static implicit operator Guid(ProductId productId) => productId.Value;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value.ToString();
}