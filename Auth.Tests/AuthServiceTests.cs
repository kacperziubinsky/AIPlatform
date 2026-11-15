using System.Security.Authentication;
using Auth.Data;
using Auth.Models.Enums;
using Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Auth.Tests;

public class AuthServiceTests
{
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<IPasswordHasher<User>> _hasher = new();
    private readonly Mock<IUserServiceClient> _users = new();

    public AuthServiceTests()
    {
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns((TestData.Token, DateTime.UtcNow.AddHours(1)));
        _hasher.Setup(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
               .Returns("hashed-password");
    }

    private static AuthDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private AuthService CreateService(AuthDbContext db) =>
        new(db, _jwt.Object, _hasher.Object, _users.Object);

    private void PasswordCheckReturns(PasswordVerificationResult result) =>
        _hasher.Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
               .Returns(result);


    [Fact]
    public async Task Login_Valid_ReturnsToken()
    {
        await using var db = CreateDb();
        await TestData.SeedUserAsync(db);
        PasswordCheckReturns(PasswordVerificationResult.Success);

        var result = await CreateService(db).LoginAsync(TestData.LoginRequest());

        Assert.Equal(TestData.Token, result.Token);
        Assert.Equal(TestData.Email, result.Email);
    }

    [Fact]
    public async Task Login_EmailIsTrimmedAndCaseInsensitive()
    {
        await using var db = CreateDb();
        await TestData.SeedUserAsync(db);
        PasswordCheckReturns(PasswordVerificationResult.Success);

        var result = await CreateService(db).LoginAsync(TestData.LoginRequest(email: "  TEST@Example.COM "));

        Assert.Equal(TestData.Email, result.Email);
    }

    [Fact]
    public async Task Login_RehashNeeded_StillLogsIn()
    {
        await using var db = CreateDb();
        await TestData.SeedUserAsync(db);
        PasswordCheckReturns(PasswordVerificationResult.SuccessRehashNeeded);

        var result = await CreateService(db).LoginAsync(TestData.LoginRequest());

        Assert.Equal(TestData.Token, result.Token);
    }

    [Fact]
    public async Task Login_UnknownEmail_Throws()
    {
        await using var db = CreateDb();

        await Assert.ThrowsAsync<InvalidCredentialException>(() =>
            CreateService(db).LoginAsync(TestData.LoginRequest(email: "missing@example.com")));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsAndIssuesNoToken()
    {
        await using var db = CreateDb();
        await TestData.SeedUserAsync(db);
        PasswordCheckReturns(PasswordVerificationResult.Failed);

        await Assert.ThrowsAsync<InvalidCredentialException>(() =>
            CreateService(db).LoginAsync(TestData.LoginRequest(password: "wrong")));

        _jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }


    [Fact]
    public async Task Register_Valid_SavesUserAndReturnsToken()
    {
        await using var db = CreateDb();

        var result = await CreateService(db)
            .RegisterAsync(TestData.RegisterRequest(email: "  New@Example.COM ", role: Role.Recruiter));

        var saved = await db.Users.SingleAsync();
        Assert.Equal("new@example.com", saved.Email);
        Assert.Equal("hashed-password", saved.Password);
        Assert.Equal(Role.Recruiter, saved.Role);
        Assert.True(saved.Active);
        Assert.Equal(TestData.Token, result.Token);
        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task Register_Valid_CreatesUserProfile()
    {
        await using var db = CreateDb();

        await CreateService(db).RegisterAsync(TestData.RegisterRequest());

        var saved = await db.Users.SingleAsync();
        _users.Verify(c => c.CreateUserAsync(saved.Id), Times.Once);
    }

    [Fact]
    public async Task Register_EmailTaken_ThrowsAndCreatesNothing()
    {
        await using var db = CreateDb();
        await TestData.SeedUserAsync(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(db).RegisterAsync(TestData.RegisterRequest(email: "TEST@example.com")));

        _users.Verify(c => c.CreateUserAsync(It.IsAny<Guid>()), Times.Never);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Register_UserServiceDown_ThrowsAndSavesNothing()
    {
        await using var db = CreateDb();
        _users.Setup(c => c.CreateUserAsync(It.IsAny<Guid>())).ThrowsAsync(new HttpRequestException());

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            CreateService(db).RegisterAsync(TestData.RegisterRequest()));

        Assert.Equal(0, await db.Users.CountAsync());
    }
}
