using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController(UserService userService) : ControllerBase
{
    /// <summary>Returns a summary list of all registered users.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await userService.GetAllUsersAsync(ct);
        return Ok(result);
    }
}
