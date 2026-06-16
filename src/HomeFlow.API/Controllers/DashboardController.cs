using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await dashboardService.GetDashboardAsync();
        return Ok(result);
    }
}
