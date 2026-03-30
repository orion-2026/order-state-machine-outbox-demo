using Microsoft.AspNetCore.Mvc;

namespace OrderStateMachineOutboxDemo.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        service = "Order State Machine + Outbox Demo",
        pattern = "State machine + application service + outbox"
    });
}
