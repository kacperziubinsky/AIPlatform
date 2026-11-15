using Microsoft.AspNetCore.Mvc;
using Moq;
using User.DTOs;
using User.Services;

namespace User.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _service = new();

    private Controllers.UserController CreateUserController(Guid? userId = null) =>
        new(_service.Object) { ControllerContext = TestData.ControllerContext(userId) };


    [Fact]
    public async Task GetAllUsers_Returns200()
    {
        var users = new List<UserResponse> { TestData.UserResponse() };
        _service.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

        var result = await CreateUserController().GetAllUsersAsync();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(users, ok.Value);
    }

    [Fact]
    public async Task GetUser_Existing_Returns200()
    {
        var id = Guid.NewGuid();
        var user = TestData.UserResponse(id);
        _service.Setup(s => s.GetUserAsync(id)).ReturnsAsync(user);

        var result = await CreateUserController().GetUserAsync(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(user, ok.Value);
    }

    [Fact]
    public async Task GetUser_Missing_Returns404()
    {
        var result = await CreateUserController().GetUserAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }


    [Fact]
    public async Task GetMe_Existing_Returns200()
    {
        var userId = Guid.NewGuid();
        var user = TestData.UserResponse(userId);
        _service.Setup(s => s.GetUserAsync(userId)).ReturnsAsync(user);

        var result = await CreateUserController(userId).GetMeAsync();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(user, ok.Value);
    }

    [Fact]
    public async Task GetMe_Missing_Returns404()
    {
        var result = await CreateUserController(Guid.NewGuid()).GetMeAsync();

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetMe_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateUserController(userId: null).GetMeAsync();

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }


    [Fact]
    public async Task UpdateMe_Valid_Returns200()
    {
        var userId = Guid.NewGuid();
        var user = TestData.UserResponse(userId);
        _service.Setup(s => s.UpdateUserAsync(userId, It.IsAny<UpdateUserRequest>())).ReturnsAsync(user);

        var result = await CreateUserController(userId).UpdateMeAsync(TestData.UpdateRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(user, ok.Value);
    }

    [Fact]
    public async Task UpdateMe_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateUserController(userId: null).UpdateMeAsync(TestData.UpdateRequest());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
        _service.Verify(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>()), Times.Never);
    }
}
