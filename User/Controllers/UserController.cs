using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.DTOs;
using User.Services;

namespace User.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsersAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<UserResponse>> GetUserAsync(Guid id)
    {
        var user = await _userService.GetUserAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMeAsync()
    {
        var userId = GetUserIdFromClaims();
        if (userId is null)
            return Unauthorized("Invalid user token claim.");

        var user = await _userService.GetUserAsync(userId.Value);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateMeAsync(UpdateUserRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null)
            return Unauthorized("Invalid user token claim.");

        var user = await _userService.UpdateUserAsync(userId.Value, request);
        return Ok(user);
    }

    private Guid? GetUserIdFromClaims()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}