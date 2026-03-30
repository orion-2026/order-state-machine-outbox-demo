using Microsoft.AspNetCore.Mvc;
using OrderStateMachineOutboxDemo.Models;
using OrderStateMachineOutboxDemo.Services;

namespace OrderStateMachineOutboxDemo.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderApplicationService _service;

    public OrdersController(OrderApplicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListOrdersAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var order = await _service.GetAsync(id);
        return order is null ? NotFound(new { message = "Order not found." }) : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId) || string.IsNullOrWhiteSpace(request.ProductSku) || request.Quantity <= 0)
        {
            return BadRequest(new { message = "CustomerId, ProductSku, and positive Quantity are required." });
        }

        var order = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet("{id:guid}/actions")]
    public async Task<IActionResult> AllowedActions(Guid id)
    {
        var actions = await _service.AllowedActionsAsync(id);
        return actions is null ? NotFound(new { message = "Order not found." }) : Ok(new { actions });
    }

    [HttpPost("{id:guid}/transitions")]
    public async Task<IActionResult> Transition(Guid id, [FromBody] ChangeStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return BadRequest(new { message = "Action is required." });
        }

        var result = await _service.ChangeStatusAsync(id, request);
        if (!result.Success)
        {
            if (result.Error == "Order not found.")
            {
                return NotFound(new { message = result.Error });
            }

            return Conflict(new { message = result.Error });
        }

        return Ok(result.Order);
    }
}
