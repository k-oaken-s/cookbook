namespace DDD.Application.Products.Commands.UpdateStock;

using DDD.Application.Common;
using System;

public record UpdateStockCommand(
    Guid ProductId,
    int Quantity,
    bool IsAddition) : CommandBase<bool>;