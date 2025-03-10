namespace DDD.Domain.Products;

using DDD.Domain.Common;

public interface IProductRepository : IRepository<Product, ProductId>
{
    Task<Product?> GetBySKUAsync(SKU sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default);
}
