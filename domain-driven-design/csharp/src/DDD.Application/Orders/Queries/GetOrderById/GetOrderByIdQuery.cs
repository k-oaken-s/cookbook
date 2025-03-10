namespace DDD.Application.Orders.Queries.GetOrderById;

using DDD.Application.Common;
using System;

public record GetOrderByIdQuery(Guid Id) : QueryBase<OrderDetailsDto?>;
