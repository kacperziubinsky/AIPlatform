using System.Security.Authentication;
using Auth.Data;
using Auth.DTOs;
using Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services;

public class AuthService: IAuthService
{
    private readonly AuthDBContext _context;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<User> _passwordHasher;
    
    public AuthService(
        AuthDBContext context,
        IJwtService jwtService,
        PasswordHasher<User> passwordHasher)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
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
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            Phone = request.Phone,
            Role = request.Role
        };
        
        user.Password = _passwordHasher.HashPassword(user, request.Password);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var token = _jwtService.GenerateToken(user);
        
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Email = user.Email
        };
    }
}