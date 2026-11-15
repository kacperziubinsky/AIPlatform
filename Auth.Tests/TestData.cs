using Auth.Data;
using Auth.DTOs;
using Auth.Models.Enums;

namespace Auth.Tests;

public static class TestData
{
    public const string Email = "test@example.com";
    public const string Password = "Password123!";
    public const string Token = "jwt-token";

    public static LoginRequest LoginRequest(
        string email = Email,
        string password = Password) => new()
    {
        Email = email,
        Password = password
    };

    public static RegisterRequest RegisterRequest(
        string email = Email,
        string password = Password,
        Role role = Role.Applicant) => new()
    {
        Email = email,
        Password = password,
        Role = role
    };

    public static AuthResponse Response(string email = Email) => new()
    {
        Token = Token,
        Email = email,
        ExpiresAt = DateTime.UtcNow.AddHours(1)
    };

    public static async Task<User> SeedUserAsync(
        AuthDbContext db,
        string email = Email,
        string passwordHash = "hashed-password",
        Role role = Role.Applicant,
        bool active = true)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Password = passwordHash,
            Role = role,
            Active = active
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
