namespace DDD.API.Controllers;

using DDD.Application.Orders.Commands.CancelOrder;
using DDD.Application.Orders.Commands.CreateOrder;
using DDD.Application.Orders.Queries.GetOrderById;
using DDD.Application.Orders.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
        [FromQuery] Guid? customerId = null,
        [FromQuery] int? statusId = null)
    {
        var query = new GetOrdersQuery(customerId, statusId);
        var orders = await _mediator.Send(query);
        
        return Ok(orders);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrderById(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query);
        
        if (order == null)
            return NotFound();
            
        return Ok(order);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateOrder(CreateOrderCommand command)
    {
        var orderId = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
    }
    
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<bool>> CancelOrder(Guid id)
    {
        var command = new CancelOrderCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result)
            return BadRequest("Failed to cancel order");
            
        return Ok(result);
    }
}