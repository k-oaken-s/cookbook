namespace DDD.Application.Products.Queries.GetProducts;

using System;
using System.Collections.Generic;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    IEnumerable<string> Categories);