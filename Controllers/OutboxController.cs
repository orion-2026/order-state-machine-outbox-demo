using Microsoft.AspNetCore.Mvc;
using OrderStateMachineOutboxDemo.Services;

namespace OrderStateMachineOutboxDemo.Controllers;

[ApiController]
[Route("api/outbox")]
public class OutboxController : ControllerBase
{
    private readonly OrderApplicationService _service;

    public OutboxController(OrderApplicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult List() => Ok(_service.ListOutbox());

    [HttpPost("publish")]
    public IActionResult PublishPending()
    {
        var published = _service.PublishPendingOutbox();
        return Ok(new
        {
            publishedCount = published.Count,
            events = published
        });
    }
}
