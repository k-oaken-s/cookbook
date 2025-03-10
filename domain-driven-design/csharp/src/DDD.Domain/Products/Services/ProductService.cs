namespace DDD.Domain.Products.Services;

using DDD.Domain.Products.Specifications;

public class ProductService
{
    private readonly IProductRepository _productRepository;
    
    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<bool> IsSkuUniqueAsync(SKU sku, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetBySKUAsync(sku, cancellationToken);
        return product == null;
    }
    
    public async Task<IEnumerable<Product>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        var specification = ProductSpecification.Active().And(new InStockProductSpecification());
        
        return await _productRepository.GetBySpecificationAsync(specification, cancellationToken);
    }
    
    public async Task<bool> CanRemoveFromStockAsync(ProductId productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return false;
            
        return product.StockQuantity >= quantity;
    }
}