namespace DDD.Application.Orders.Commands.CancelOrder;

using DDD.Application.Common;
using DDD.Domain.Orders;
using DDD.Domain.Orders.Services;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CancelOrderCommandHandler : CommandHandlerBase<CancelOrderCommand, bool>
{
    private readonly OrderService _orderService;
    
    public CancelOrderCommandHandler(
        IUnitOfWork unitOfWork,
        OrderService orderService) : base(unitOfWork)
    {
        _orderService = orderService;
    }
    
    public override async Task<bool> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Create(command.OrderId);
        
        return await UnitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            var result = await _orderService.CancelOrderAsync(orderId, cancellationToken);
            
            if (result)
            {
                await UnitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            
            return false;
        }, cancellationToken) ?? false;
    }
}