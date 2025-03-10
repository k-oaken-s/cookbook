namespace DDD.Application.Orders.Commands.CreateOrder;

using DDD.Application.Common;
using DDD.Domain.Customers;
using DDD.Domain.Orders.Services;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CreateOrderCommandHandler : CommandHandlerBase<CreateOrderCommand, Guid>
{
    private readonly OrderService _orderService;
    
    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        OrderService orderService) : base(unitOfWork)
    {
        _orderService = orderService;
    }
    
    public override async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(command.CustomerId);
        
        // 配送先住所の変換
        Address? shippingAddress = null;
        if (command.ShippingAddress != null)
        {
            shippingAddress = Address.Create(
                command.ShippingAddress.Street,
                command.ShippingAddress.City,
                command.ShippingAddress.State,
                command.ShippingAddress.Country,
                command.ShippingAddress.ZipCode);
        }
        
        // 注文アイテムの辞書を作成
        var orderItems = command.Items
            .ToDictionary(
                i => Domain.Products.ProductId.Create(i.ProductId),
                i => i.Quantity);
                
        // トランザクション内で注文を作成
        return await UnitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            var order = await _orderService.CreateOrderAsync(
                customerId,
                orderItems,
                shippingAddress,
                cancellationToken);
                
            if (order == null)
                throw new InvalidOperationException("Failed to create order");
                
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            
            return order.Id.Value;
        }, cancellationToken) ?? Guid.Empty;
    }
}