using CreativesIntegration.Api.Models;
using CreativesIntegration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreativesIntegration.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController(IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await userService.LoginAsync(request.Username, request.Password);

        if (token is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(new LoginResponse(token));
    }
}