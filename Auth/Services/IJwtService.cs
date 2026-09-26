using Auth.Models;

namespace Auth.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}