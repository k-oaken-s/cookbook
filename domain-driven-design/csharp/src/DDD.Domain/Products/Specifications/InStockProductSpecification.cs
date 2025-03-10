namespace DDD.Domain.Products.Specifications;

using DDD.Domain.Common;

public class InStockProductSpecification : Specification<Product>
{
    private readonly int _minStock;
    
    public InStockProductSpecification(int minStock = 1)
    {
        _minStock = minStock;
    }
    
    public override bool IsSatisfiedBy(Product entity)
    {
        return entity.StockQuantity >= _minStock;
    }
}