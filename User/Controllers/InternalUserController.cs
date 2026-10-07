using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using User.Services;

namespace User.Controllers;

[ApiController]
[Route("internal/users")]
public class InternalUserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public InternalUserController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("{userId:guid}")]
    public async Task<IActionResult> CreateUserAsync(Guid userId)
    {
        if (!IsValidApiKey())
            return Unauthorized();

        if (userId == Guid.Empty)
            return BadRequest("UserId is required.");

        var created = await _userService.CreateUserAsync(userId);
        return created ? StatusCode(201) : NoContent();
    }

    private bool IsValidApiKey()
    {
        var expected = _configuration["InternalApiKey"];
        var provided = Request.Headers["X-Api-Key"].ToString();

        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(provided))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(provided));
    }
}