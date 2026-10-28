using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Data;
using Auth.Models;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),
            new Claim(
                ClaimTypes.Email,
                user.Email),
            new Claim(ClaimTypes.Role,
                user.Role.ToString()),
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ?? string.Empty));
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.TryParse(_configuration["JWT:ExpiryMinutes"], out var minutes) ? minutes : 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);
        
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);

    }
}