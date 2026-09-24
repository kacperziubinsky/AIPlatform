using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = Microsoft.AspNetCore.Identity.Data.LoginRequest;

namespace Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Email != "" && request.Password != "")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, request.Email),
                new Claim(ClaimTypes.Role, "Admin"),
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6ymfS6QVPkpRcVXlPbt7wzQRopENIs3Nx6etwErkBOt"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "AI Platform",
                audience: "AI Platform",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        return Unauthorized();
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User request)
    {
        if (request.Email != "" || request.Password != "")
        {
            return BadRequest();
        }
        
        
    }
}