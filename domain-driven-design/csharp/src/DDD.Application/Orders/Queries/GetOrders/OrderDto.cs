namespace DDD.Application.Orders.Queries.GetOrders;

using System;
using System.Collections.Generic;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    string Currency);