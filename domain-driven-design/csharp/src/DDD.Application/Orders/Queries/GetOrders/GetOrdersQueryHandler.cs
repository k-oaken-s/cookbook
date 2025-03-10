namespace DDD.Application.Orders.Queries.GetOrders;

using DDD.Application.Common;
using DDD.Domain.Customers;
using DDD.Domain.Orders;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetOrdersQueryHandler : QueryHandlerBase<GetOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    
    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }
    
    public override async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<Order> orders;
        
        if (query.CustomerId.HasValue)
        {
            var customerId = CustomerId.Create(query.CustomerId.Value);
            orders = await _orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        }
        else if (query.StatusId.HasValue)
        {
            var status = OrderStatus.FromId<OrderStatus>(query.StatusId.Value);
            orders = await _orderRepository.GetByStatusAsync(status, cancellationToken);
        }
        else
        {
            orders = await _orderRepository.GetAllAsync(cancellationToken);
        }
        
        var result = new List<OrderDto>();
        
        foreach (var order in orders)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
            var customerName = customer != null ? customer.FullName : "Unknown";
            
            var totalMoney = order.GetTotal();
            
            result.Add(new OrderDto(
                order.Id.Value,
                order.CustomerId.Value,
                customerName,
                order.OrderDate,
                order.Status.Name,
                totalMoney.Amount,
                totalMoney.Currency));
        }
        
        return result;
    }
}
