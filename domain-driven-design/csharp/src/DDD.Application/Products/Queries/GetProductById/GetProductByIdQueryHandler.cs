namespace DDD.Application.Products.Queries.GetProductById;

using DDD.Application.Common;
using DDD.Domain.Products;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetProductByIdQueryHandler : QueryHandlerBase<GetProductByIdQuery, ProductDetailsDto?>
{
    private readonly IProductRepository _productRepository;
    
    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    public override async Task<ProductDetailsDto?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var productId = ProductId.Create(query.Id);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return null;
            
        return new ProductDetailsDto(
            product.Id.Value,
            product.Name,
            product.Description,
            product.Sku.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.StockQuantity,
            product.IsActive,
            DateTime.UtcNow,  // 実際にはエンティティに作成日時を持たせる
            product.Categories.Select(c => new CategoryDto(c.Id, c.Name)));
    }
}