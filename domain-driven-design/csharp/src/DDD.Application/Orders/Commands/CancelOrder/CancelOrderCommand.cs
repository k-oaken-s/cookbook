namespace DDD.Application.Orders.Commands.CancelOrder;

using DDD.Application.Common;
using System;

public record CancelOrderCommand(Guid OrderId) : CommandBase<bool>;
