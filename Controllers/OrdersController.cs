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
    public IActionResult List() => Ok(_service.ListOrders());

    [HttpGet("{id:guid}")]
    public IActionResult Get(Guid id)
    {
        var order = _service.Get(id);
        return order is null ? NotFound(new { message = "Order not found." }) : Ok(order);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId) || string.IsNullOrWhiteSpace(request.ProductSku) || request.Quantity <= 0)
        {
            return BadRequest(new { message = "CustomerId, ProductSku, and positive Quantity are required." });
        }

        var order = _service.Create(request);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet("{id:guid}/actions")]
    public IActionResult AllowedActions(Guid id)
    {
        var actions = _service.AllowedActions(id);
        return actions is null ? NotFound(new { message = "Order not found." }) : Ok(new { actions });
    }

    [HttpPost("{id:guid}/transitions")]
    public IActionResult Transition(Guid id, [FromBody] ChangeStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return BadRequest(new { message = "Action is required." });
        }

        var result = _service.ChangeStatus(id, request);
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
