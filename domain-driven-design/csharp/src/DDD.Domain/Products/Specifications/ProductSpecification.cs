namespace DDD.Domain.Products.Specifications;

using DDD.Domain.Common;

public class ProductSpecification : Specification<Product>
{
    public static ProductSpecification ByCategory(Category category)
    {
        return new ProductSpecification(p => p.Categories.Contains(category));
    }
    
    public static ProductSpecification Active()
    {
        return new ProductSpecification(p => p.IsActive);
    }
    
    public static ProductSpecification ByMinPrice(Price minPrice)
    {
        return new ProductSpecification(p => p.Price.Amount >= minPrice.Amount && p.Price.Currency == minPrice.Currency);
    }
    
    public static ProductSpecification ByMaxPrice(Price maxPrice)
    {
        return new ProductSpecification(p => p.Price.Amount <= maxPrice.Amount && p.Price.Currency == maxPrice.Currency);
    }
    
    private readonly Func<Product, bool> _expression;
    
    private ProductSpecification(Func<Product, bool> expression)
    {
        _expression = expression;
    }
    
    public override bool IsSatisfiedBy(Product entity)
    {
        return _expression(entity);
    }
}
