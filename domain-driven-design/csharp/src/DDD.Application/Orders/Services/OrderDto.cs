namespace DDD.Application.Orders.Services;

using System;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    string Currency);