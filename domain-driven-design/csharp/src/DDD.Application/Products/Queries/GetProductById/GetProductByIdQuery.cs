namespace DDD.Application.Products.Queries.GetProductById;

using DDD.Application.Common;
using System;

public record GetProductByIdQuery(Guid Id) : QueryBase<ProductDetailsDto?>;
