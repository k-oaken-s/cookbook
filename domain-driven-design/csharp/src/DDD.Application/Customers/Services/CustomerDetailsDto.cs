namespace DDD.Application.Customers.Services;

using System;
using System.Collections.Generic;

public record CustomerDetailsDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    AddressDto? DefaultShippingAddress,
    IEnumerable<CustomerOrderDto> Orders);
    
public record CustomerOrderDto(
    Guid Id,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount);