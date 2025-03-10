namespace DDD.Application.Products.Commands.CreateProduct;

using DDD.Application.Common;
using DDD.Domain.Products;
using DDD.Domain.Products.Factories;
using DDD.Domain.Products.Services;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CreateProductCommandHandler : CommandHandlerBase<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ProductService _productService;
    
    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IProductRepository productRepository,
        ProductService productService) : base(unitOfWork)
    {
        _productRepository = productRepository;
        _productService = productService;
    }
    
    public override async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var sku = SKU.Create(command.Sku);
        
        // SKUの一意性検証
        if (!await _productService.IsSkuUniqueAsync(sku, cancellationToken))
        {
            throw new InvalidOperationException($"SKU '{sku}' is already in use");
        }
        
        // カテゴリのリストを取得（該当する場合）
        var categories = new List<Category>();
        if (command.CategoryIds != null)
        {
            foreach (var categoryId in command.CategoryIds)
            {
                // ここでは簡略化のためにカテゴリは存在するものとします
                // 実際にはカテゴリリポジトリから取得する必要があります
                categories.Add(new Category(categoryId, $"Category {categoryId}", "Description"));
            }
        }
        
        // 商品の作成
        var product = ProductFactory.CreateProduct(
            command.Name,
            command.Description,
            command.Sku,
            command.Price,
            command.Currency,
            command.StockQuantity,
            command.IsActive);
            
        // カテゴリを追加
        foreach (var category in categories)
        {
            product.AddCategory(category);
        }
        
        // トランザクション内で商品を保存
        return await UnitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            await _productRepository.AddAsync(product, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            
            return product.Id.Value;
        }, cancellationToken) ?? Guid.Empty;
    }
}