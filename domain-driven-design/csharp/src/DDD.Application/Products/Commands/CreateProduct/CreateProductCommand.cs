namespace DDD.Application.Products.Commands.CreateProduct;

using DDD.Application.Common;
using System.Collections.Generic;

public record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    List<Guid>? CategoryIds = null) : CommandBase<Guid>;
