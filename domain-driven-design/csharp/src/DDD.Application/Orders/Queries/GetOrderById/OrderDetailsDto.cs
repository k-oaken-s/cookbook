namespace DDD.Application.Orders.Queries.GetOrderById;

using System;
using System.Collections.Generic;

public record OrderDetailsDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    string Currency,
    AddressDto? ShippingAddress,
    IEnumerable<OrderLineItemDto> Items);
    
public record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);
    
public record OrderLineItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal TotalPrice);