namespace DDD.Application.Orders.Services;

using DDD.Domain.Customers;
using DDD.Domain.Orders;
using DDD.Domain.Orders.Services;
using DDD.Domain.Products;
using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class OrderApplicationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly OrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderApplicationService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        OrderService orderService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _orderService = orderService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(
        Guid? customerId = null,
        int? statusId = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Order> orders;
        
        if (customerId.HasValue)
        {
            var customerIdObj = CustomerId.Create(customerId.Value);
            orders = await _orderRepository.GetByCustomerIdAsync(customerIdObj, cancellationToken);
        }
        else if (statusId.HasValue)
        {
            var status = OrderStatus.FromId<OrderStatus>(statusId.Value);
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
    
    public async Task<OrderDetailsDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.Create(id);
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
    
    public async Task<Guid> CreateOrderAsync(
        Guid customerId,
        List<OrderItemDto> items,
        AddressDto? shippingAddress = null,
        CancellationToken cancellationToken = default)
    {
        var customerIdObj = CustomerId.Create(customerId);
        
        // 配送先住所の変換
        Address? shippingAddressObj = null;
        if (shippingAddress != null)
        {
            shippingAddressObj = Address.Create(
                shippingAddress.Street,
                shippingAddress.City,
                shippingAddress.State,
                shippingAddress.Country,
                shippingAddress.ZipCode);
        }
        
        // 注文アイテムの辞書を作成
        var orderItems = items
            .ToDictionary(
                i => Domain.Products.ProductId.Create(i.ProductId),
                i => i.Quantity);
                
        // トランザクション内で注文を作成
        var order = await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            var newOrder = await _orderService.CreateOrderAsync(
                customerIdObj,
                orderItems,
                shippingAddressObj,
                cancellationToken);
                
            if (newOrder == null)
                throw new InvalidOperationException("Failed to create order");
                
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return newOrder;
        }, cancellationToken);
        
        if (order == null)
            throw new InvalidOperationException("Failed to create order");
            
        return order.Id.Value;
    }
    
    public async Task<bool> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.Create(id);
        
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            var result = await _orderService.CancelOrderAsync(orderId, cancellationToken);
            
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            
            return false;
        }, cancellationToken) ?? false;
    }
    
    public async Task<bool> ProcessOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.Create(id);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
            return false;
            
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            try
            {
                order.Process();
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }, cancellationToken) ?? false;
    }
    
    public async Task<bool> ShipOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.Create(id);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
            return false;
            
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            try
            {
                order.Ship();
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }, cancellationToken) ?? false;
    }
    
    public async Task<bool> DeliverOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.Create(id);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
            return false;
            
        return await _unitOfWork.ExecuteWithTransactionAsync(async () =>
        {
            try
            {
                order.Deliver();
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }, cancellationToken) ?? false;
    }
}