namespace DDD.Application.Products.Queries.GetProducts;

using DDD.Application.Common;
using DDD.Domain.Products;
using DDD.Domain.Products.Specifications;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetProductsQueryHandler : QueryHandlerBase<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    
    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    public override async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        
        if (query.ActiveOnly)
        {
            products = products.Where(p => p.IsActive);
        }
        
        return products.Select(p => new ProductDto(
            p.Id.Value,
            p.Name,
            p.Description,
            p.Sku.Value,
            p.Price.Amount,
            p.Price.Currency,
            p.StockQuantity,
            p.IsActive,
            p.Categories.Select(c => c.Name)));
    }
}