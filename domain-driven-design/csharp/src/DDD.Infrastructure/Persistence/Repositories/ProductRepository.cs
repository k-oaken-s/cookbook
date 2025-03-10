namespace DDD.Infrastructure.Persistence.Repositories;

using DDD.Domain.Common;
using DDD.Domain.Products;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;
    
    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Categories)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Product?> GetBySKUAsync(SKU sku, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Sku.Value == sku.Value, cancellationToken);
    }
    
    public async Task<IEnumerable<Product>> GetBySpecificationAsync(
        ISpecification<Product> specification,
        CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .Include(p => p.Categories)
            .ToListAsync(cancellationToken);
            
        return products.Where(specification.IsSatisfiedBy);
    }
    
    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(entity, cancellationToken);
    }
    
    public Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Update(entity);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Remove(entity);
        return Task.CompletedTask;
    }
}