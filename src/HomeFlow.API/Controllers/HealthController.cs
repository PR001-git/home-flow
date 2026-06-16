using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>Returns a 200 OK response indicating the API is running.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
