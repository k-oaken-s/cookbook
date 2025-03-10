namespace DDD.Application.Orders.Queries.GetOrderById;

using DDD.Application.Common;
using DDD.Domain.Customers;
using DDD.Domain.Orders;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetOrderByIdQueryHandler : QueryHandlerBase<GetOrderByIdQuery, OrderDetailsDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    
    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }
    
    public override async Task<OrderDetailsDto?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Create(query.Id);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
            return null;
            
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customer == null)
            return null;
            
        AddressDto? shippingAddressDto = null;
        if (order.ShippingAddress != null)
        {
            shippingAddressDto = new AddressDto(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.Country,
                order.ShippingAddress.ZipCode);
        }
        
        var totalMoney = order.GetTotal();
        
        var orderLineItems = order.OrderLines.Select(ol =>
        {
            var lineTotalMoney = ol.GetTotal();
            
            return new OrderLineItemDto(
                ol.Id,
                ol.ProductId.Value,
                ol.ProductName,
                ol.Quantity,
                ol.UnitPrice.Amount,
                ol.UnitPrice.Currency,
                lineTotalMoney.Amount);
        });
        
        return new OrderDetailsDto(
            order.Id.Value,
            customer.Id.Value,
            customer.FullName,
            customer.Email.Value,
            order.OrderDate,
            order.Status.Name,
            totalMoney.Amount,
            totalMoney.Currency,
            shippingAddressDto,
            orderLineItems);
    }
}