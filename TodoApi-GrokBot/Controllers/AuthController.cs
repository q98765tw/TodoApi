using Microsoft.AspNetCore.Mvc;
using TodoApiGrokBot.Models.Dtos;
using TodoApiGrokBot.Services;

namespace TodoApiGrokBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(AuthRequest req)
    {
        var result = await auth.RegisterAsync(req);
        return result.Succeeded ? StatusCode(201, result.User) : BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(AuthRequest req) =>
        await auth.LoginAsync(req) is { } token ? Ok(token) : Unauthorized();
}
