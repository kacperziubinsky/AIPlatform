using System.Security.Authentication;
using Auth.DTOs;
using Auth.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Auth.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _service = new();

    private Controllers.AuthController CreateAuthController() => new(_service.Object);


    [Fact]
    public async Task Login_Valid_Returns200()
    {
        var response = TestData.Response();
        _service.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(response);

        var result = await CreateAuthController().Login(TestData.LoginRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        _service.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ThrowsAsync(new InvalidCredentialException());

        var result = await CreateAuthController().Login(TestData.LoginRequest(password: "wrong"));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Invalid email or password.", unauthorized.Value);
    }


    [Fact]
    public async Task Register_Valid_Returns200()
    {
        var response = TestData.Response();
        _service.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>())).ReturnsAsync(response);

        var result = await CreateAuthController().Register(TestData.RegisterRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task Register_EmailTaken_Returns409()
    {
        _service.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ThrowsAsync(new InvalidOperationException("Email already exists."));

        var result = await CreateAuthController().Register(TestData.RegisterRequest());

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("Email already exists.", conflict.Value);
    }

    [Fact]
    public async Task Register_UserServiceDown_Returns502()
    {
        _service.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ThrowsAsync(new HttpRequestException());

        var result = await CreateAuthController().Register(TestData.RegisterRequest());

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, obj.StatusCode);
    }
}
