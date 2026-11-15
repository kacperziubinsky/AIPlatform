using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Models.Enums;
using Auth.Services;
using Microsoft.Extensions.Configuration;

namespace Auth.Tests;

public class JwtServiceTests
{
    private static JwtService CreateService(string? expiryMinutes = "30") =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT:Key"] = "test-signing-key-that-is-long-enough-for-hs256",
                ["JWT:Issuer"] = "auth",
                ["JWT:Audience"] = "platform",
                ["JWT:ExpiryMinutes"] = expiryMinutes
            })
            .Build());

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = TestData.Email,
        Role = Role.Recruiter
    };


    [Fact]
    public void GenerateToken_ContainsUserClaims()
    {
        var user = CreateUser();

        var (token, _) = CreateService().GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal("auth", jwt.Issuer);
        Assert.Contains("platform", jwt.Audiences);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Email && c.Value == TestData.Email);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Recruiter");
    }

    [Fact]
    public void GenerateToken_UsesConfiguredExpiry()
    {
        var (_, expiresAt) = CreateService(expiryMinutes: "30").GenerateToken(CreateUser());

        Assert.InRange(expiresAt, DateTime.UtcNow.AddMinutes(29), DateTime.UtcNow.AddMinutes(31));
    }

    [Fact]
    public void GenerateToken_MissingExpiry_DefaultsTo60Minutes()
    {
        var (_, expiresAt) = CreateService(expiryMinutes: null).GenerateToken(CreateUser());

        Assert.InRange(expiresAt, DateTime.UtcNow.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
    }
}
