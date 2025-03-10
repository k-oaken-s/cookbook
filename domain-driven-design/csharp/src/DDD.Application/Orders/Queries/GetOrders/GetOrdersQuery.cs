namespace DDD.Application.Orders.Queries.GetOrders;

using DDD.Application.Common;
using DDD.Domain.Orders;
using System;
using System.Collections.Generic;

public record GetOrdersQuery(
    Guid? CustomerId = null,
    int? StatusId = null) : QueryBase<IEnumerable<OrderDto>>;