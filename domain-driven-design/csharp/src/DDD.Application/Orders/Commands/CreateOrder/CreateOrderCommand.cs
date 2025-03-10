namespace DDD.Application.Orders.Commands.CreateOrder;

using DDD.Application.Common;
using System;
using System.Collections.Generic;

public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderItemDto> Items,
    ShippingAddressDto? ShippingAddress = null) : CommandBase<Guid>;
    
public record OrderItemDto(Guid ProductId, int Quantity);

public record ShippingAddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);