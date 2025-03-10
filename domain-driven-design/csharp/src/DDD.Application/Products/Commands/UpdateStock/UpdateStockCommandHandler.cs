namespace DDD.Application.Products.Commands.UpdateStock;

using DDD.Application.Common;
using DDD.Domain.Products;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

public class UpdateStockCommandHandler : CommandHandlerBase<UpdateStockCommand, bool>
{
    private readonly IProductRepository _productRepository;
    
    public UpdateStockCommandHandler(
        IUnitOfWork unitOfWork,
        IProductRepository productRepository) : base(unitOfWork)
    {
        _productRepository = productRepository;
    }
    
    public override async Task<bool> Handle(UpdateStockCommand command, CancellationToken cancellationToken)
    {
        var productId = ProductId.Create(command.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found");
        }
        
        return await UnitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            if (command.IsAddition)
            {
                product.AddToStock(command.Quantity);
            }
            else
            {
                if (!product.RemoveFromStock(command.Quantity))
                {
                    return false;
                }
            }
            
            await _productRepository.UpdateAsync(product, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }, cancellationToken) ?? false;
    }
}