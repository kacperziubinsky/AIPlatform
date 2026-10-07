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
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IUserServiceClient _userServiceClient;
    
    public AuthService(
        AuthDbContext context,
        IJwtService jwtService,
        PasswordHasher<User> passwordHasher, 
        IUserServiceClient userServiceClient)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _userServiceClient = userServiceClient;
    }
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
       if (user is null)
       {
           throw new InvalidCredentialException();
       }
       
       var results = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

       if (results == PasswordVerificationResult.Failed)
       {
           throw new InvalidCredentialException();
       }
       
       var token = _jwtService.GenerateToken(user);

       return new AuthResponse
       {
           Token = token,
           ExpiresAt = DateTime.UtcNow.AddHours(1),
           Email = user.Email
       };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);

        if (exists)
        {
            throw new Exception("Email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Role = request.Role
        };
        
        user.Password = _passwordHasher.HashPassword(user, request.Password);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        try
        {
            await _userServiceClient.CreateUserAsync(user.Id);
        }
        catch
        {
            throw new Exception("Failed to create user");
        }
        
        var token = _jwtService.GenerateToken(user);
        
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Email = user.Email
        };
    }
}