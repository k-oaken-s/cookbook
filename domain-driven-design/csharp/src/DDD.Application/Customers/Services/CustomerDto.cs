namespace DDD.Application.Customers.Services;

using System;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    AddressDto? DefaultShippingAddress);
    
public record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);