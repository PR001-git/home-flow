using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await userService.GetAllUsersAsync();
        return Ok(result);
    }
}
