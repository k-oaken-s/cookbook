namespace DDD.Domain.Orders;

using DDD.Domain.Common;
using DDD.Domain.Customers;
using DDD.Domain.Orders.Events;
using System;
using System.Collections.Generic;
using System.Linq;

public class Order : Entity<OrderId>, IAggregateRoot
{
    public CustomerId CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address? ShippingAddress { get; private set; }
    
    // ファーストクラスコレクション
    private readonly List<OrderLine> _orderLines = new();
    public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();
    
    public Order(OrderId id, CustomerId customerId, Address? shippingAddress = null)
    {
        Id = id;
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        ShippingAddress = shippingAddress;
        
        AddDomainEvent(new OrderCreatedEvent(id, customerId));
    }
    
    private Order() { } // For EF Core
    
    public void AddOrderLine(ProductId productId, string productName, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify order once it has been processed");
            
        var existingLine = _orderLines.FirstOrDefault(ol => ol.ProductId == productId);
        if (existingLine != null)
        {
            existingLine.UpdateQuantity(existingLine.Quantity + quantity);
        }
        else
        {
            var orderLine = new OrderLine(Guid.NewGuid(), productId, productName, quantity, unitPrice);
            _orderLines.Add(orderLine);
        }
    }
    
    public void RemoveOrderLine(Guid orderLineId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify order once it has been processed");
            
        var orderLine = _orderLines.FirstOrDefault(ol => ol.Id == orderLineId);
        if (orderLine != null)
        {
            _orderLines.Remove(orderLine);
        }
    }
    
    public Money GetTotal()
    {
        if (!_orderLines.Any())
            return Money.Zero("USD");
            
        var firstLine = _orderLines.First();
        var total = Money.Zero(firstLine.UnitPrice.Currency);
        
        foreach (var line in _orderLines)
        {
            total = total.Add(line.GetTotal());
        }
        
        return total;
    }
    
    public void Process()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order must be in Pending status to be processed");
            
        if (!_orderLines.Any())
            throw new InvalidOperationException("Cannot process an empty order");
            
        Status = OrderStatus.Processing;
    }
    
    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Order must be in Processing status to be shipped");
            
        Status = OrderStatus.Shipped;
    }
    
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Order must be in Shipped status to be delivered");
            
        Status = OrderStatus.Delivered;
    }
    
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered order");
            
        Status = OrderStatus.Cancelled;
        
        AddDomainEvent(new OrderCancelledEvent(Id));
    }
    
    public void UpdateShippingAddress(Address address)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Processing)
            throw new InvalidOperationException("Cannot update shipping address after order has been shipped");
            
        ShippingAddress = address;
    }
}