using System.Security.Claims;
using HomeFlow.Application.DTOs.Auth;
using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(UserService userService) : ControllerBase
{
    /// <summary>Registers a new user and returns a JWT token.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await userService.RegisterAsync(request, ct);
        return Created($"/api/auth/me", result);
    }

    /// <summary>Authenticates a user by username and password and returns a JWT token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await userService.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await userService.GetByIdAsync(userId, ct);
        return Ok(result);
    }
}
