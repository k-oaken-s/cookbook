namespace DDD.Application.Products.Queries.GetProducts;

using DDD.Application.Common;
using System.Collections.Generic;

public record GetProductsQuery(
    bool ActiveOnly = false) : QueryBase<IEnumerable<ProductDto>>;
