namespace DDD.API.Controllers;

using DDD.Application.Products.Commands.CreateProduct;
using DDD.Application.Products.Commands.UpdateStock;
using DDD.Application.Products.Queries.GetProductById;
using DDD.Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] bool activeOnly = false)
    {
        var query = new GetProductsQuery(activeOnly);
        var products = await _mediator.Send(query);
        
        return Ok(products);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailsDto>> GetProductById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var product = await _mediator.Send(query);
        
        if (product == null)
            return NotFound();
            
        return Ok(product);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateProduct(CreateProductCommand command)
    {
        var productId = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetProductById), new { id = productId }, productId);
    }
    
    [HttpPut("{id}/stock")]
    public async Task<ActionResult<bool>> UpdateStock(Guid id, UpdateStockRequest request)
    {
        var command = new UpdateStockCommand(id, request.Quantity, request.IsAddition);
        var result = await _mediator.Send(command);
        
        if (!result)
            return BadRequest("Failed to update stock");
            
        return Ok(result);
    }
}