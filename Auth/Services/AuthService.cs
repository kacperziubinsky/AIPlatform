using System.Security.Authentication;
using Auth.Data;
using Auth.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services;

public class AuthService: IAuthService
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IUserServiceClient _userServiceClient;

    public AuthService(
        AuthDbContext context,
        IJwtService jwtService,
        IPasswordHasher<User> passwordHasher,
        IUserServiceClient userServiceClient)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _userServiceClient = userServiceClient;
    }
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

       if (user is null)
       {
           throw new InvalidCredentialException();
       }

       var results = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

       if (results == PasswordVerificationResult.Failed)
       {
           throw new InvalidCredentialException();
       }

       return CreateResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email);

        if (exists)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = request.Role,
            Active = true
        };

        user.Password = _passwordHasher.HashPassword(user, request.Password);

        await _userServiceClient.CreateUserAsync(user.Id);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreateResponse(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var (token, expiresAt) = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Email = user.Email
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
