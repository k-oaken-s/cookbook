namespace DDD.Application.Products.Queries.GetProductById;

using System;
using System.Collections.Generic;

public record ProductDetailsDto(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<CategoryDto> Categories);
    
public record CategoryDto(Guid Id, string Name);