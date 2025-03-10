namespace DDD.Application.Products.Services;

using DDD.Domain.Products;
using DDD.Domain.Products.Factories;
using DDD.Domain.Products.Services;
using DDD.Domain.Products.Specifications;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ProductApplicationService
{
    private readonly IProductRepository _productRepository;
    private readonly ProductService _productService;
    private readonly IUnitOfWork _unitOfWork;
    
    public ProductApplicationService(
        IProductRepository productRepository,
        ProductService productService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productService = productService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        
        if (activeOnly)
        {
            products = products.Where(p => p.IsActive);
        }
        
        return products.Select(p => MapToDto(p));
    }
    
    public async Task<ProductDetailsDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productId = ProductId.Create(id);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return null;
            
        return MapToDetailsDto(product);
    }
    
    public async Task<IEnumerable<ProductDto>> GetProductsBySpecificationAsync(bool activeOnly, decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default)
    {
        // スペックオブジェクトを構築
        ISpecification<Product> specification = new TrueSpecification<Product>();
        
        if (activeOnly)
        {
            specification = specification.And(ProductSpecification.Active());
        }
        
        if (minPrice.HasValue)
        {
            var minPriceObj = Price.Create(minPrice.Value, "USD");
            specification = specification.And(ProductSpecification.ByMinPrice(minPriceObj));
        }
        
        if (maxPrice.HasValue)
        {
            var maxPriceObj = Price.Create(maxPrice.Value, "USD");
            specification = specification.And(ProductSpecification.ByMaxPrice(maxPriceObj));
        }
        
        var products = await _productRepository.GetBySpecificationAsync(specification, cancellationToken);
        
        return products.Select(p => MapToDto(p));
    }
    
    public async Task<Guid> CreateProductAsync(
        string name,
        string description,
        string sku,
        decimal price,
        string currency,
        int stockQuantity,
        bool isActive,
        List<Guid>? categoryIds = null,
        CancellationToken cancellationToken = default)
    {
        var skuObj = SKU.Create(sku);
        
        // SKUの一意性を検証
        if (!await _productService.IsSkuUniqueAsync(skuObj, cancellationToken))
        {
            throw new InvalidOperationException($"SKU '{sku}' is already in use");
        }
        
        // カテゴリのリストを取得（該当する場合）
        var categories = new List<Category>();
        if (categoryIds != null)
        {
            foreach (var categoryId in categoryIds)
            {
                // ここでは簡略化のためにカテゴリは存在するものとします
                // 実際にはカテゴリリポジトリから取得する必要があります
                categories.Add(new Category(categoryId, $"Category {categoryId}", "Description"));
            }
        }
        
        // 製品の作成
        var product = ProductFactory.CreateProduct(
            name,
            description,
            sku,
            price,
            currency,
            stockQuantity,
            isActive);
            
        // カテゴリを追加
        foreach (var category in categories)
        {
            product.AddCategory(category);
        }
        
        // トランザクション内で保存
        await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return product.Id.Value;
        }, cancellationToken);
        
        return product.Id.Value;
    }
    
    public async Task<bool> UpdateProductDetailsAsync(
        Guid id,
        string name,
        string description,
        decimal price,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var productId = ProductId.Create(id);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return false;
            
        var priceObj = Price.Create(price, currency);
        
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            product.UpdateDetails(name, description, priceObj);
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }, cancellationToken) ?? false;
    }
    
    public async Task<bool> UpdateProductStockAsync(
        Guid id,
        int quantity,
        bool isAddition,
        CancellationToken cancellationToken = default)
    {
        var productId = ProductId.Create(id);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return false;
            
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            if (isAddition)
            {
                product.AddToStock(quantity);
            }
            else
            {
                if (!product.RemoveFromStock(quantity))
                {
                    return false;
                }
            }
            
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }, cancellationToken) ?? false;
    }
    
    public async Task<bool> ActivateProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productId = ProductId.Create(id);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return false;
            
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            product.Activate();
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }, cancellationToken) ?? false;
    }
    
    public async Task<bool> DeactivateProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productId = ProductId.Create(id);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return false;
            
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            product.Deactivate();
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }, cancellationToken) ?? false;
    }
    
    // DTOへのマッピングメソッド
    private ProductDto MapToDto(Product product)
    {
        return new ProductDto(
            product.Id.Value,
            product.Name,
            product.Description,
            product.Sku.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.StockQuantity,
            product.IsActive,
            product.Categories.Select(c => c.Name));
    }
    
    private ProductDetailsDto MapToDetailsDto(Product product)
    {
        return new ProductDetailsDto(
            product.Id.Value,
            product.Name,
            product.Description,
            product.Sku.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.StockQuantity,
            product.IsActive,
            DateTime.UtcNow, // 実際にはエンティティに作成日時を持たせる
            product.Categories.Select(c => new CategoryDto(c.Id, c.Name)));
    }
}

// TrueSpecification for base case
public class TrueSpecification<T> : Domain.Common.Specification<T>
{
    public override bool IsSatisfiedBy(T entity)
    {
        return true;
    }
}
