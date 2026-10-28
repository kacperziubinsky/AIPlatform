using Auth.Models;

namespace Auth.Services;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}