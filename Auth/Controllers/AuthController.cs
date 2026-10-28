using System.Security.Authentication;
using Auth.DTOs;
using Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _authService.LoginAsync(request));
        }
        catch (InvalidCredentialException)
        {
            return Unauthorized("Invalid email or password.");
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request)
    {
        try
        {
            return Ok(await _authService.RegisterAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to create user profile. Try again later.");
        }
    }
}