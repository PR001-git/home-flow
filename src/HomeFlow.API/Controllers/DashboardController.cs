using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    /// <summary>Returns the household dashboard summary including today's tasks, overdue count, status totals, and per-member distribution.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await dashboardService.GetDashboardAsync(ct);
        return Ok(result);
    }
}
