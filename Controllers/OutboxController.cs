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
    public async Task<IActionResult> List() => Ok(await _service.ListOutboxAsync());

    [HttpPost("publish")]
    public async Task<IActionResult> PublishPending()
    {
        var published = await _service.PublishPendingOutboxAsync();
        return Ok(new
        {
            publishedCount = published.Count,
            events = published
        });
    }
}
