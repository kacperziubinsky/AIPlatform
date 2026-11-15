using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using User.Services;

namespace User.Tests;

public class InternalUserControllerTests
{
    private const string ApiKey = "secret-key";

    private readonly Mock<IUserService> _users = new();
    private readonly Mock<ICvService> _cvs = new();

    private Controllers.InternalUserController CreateInternalController(string? apiKey = ApiKey)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["InternalApiKey"] = ApiKey })
            .Build();

        return new(_users.Object, _cvs.Object, config)
        {
            ControllerContext = TestData.ControllerContext(apiKey: apiKey)
        };
    }


    [Fact]
    public async Task CreateUser_New_Returns201()
    {
        var id = Guid.NewGuid();
        _users.Setup(s => s.CreateUserAsync(id)).ReturnsAsync(true);

        var result = await CreateInternalController().CreateUserAsync(id);

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status201Created, status.StatusCode);
    }

    [Fact]
    public async Task CreateUser_AlreadyExists_Returns204()
    {
        _users.Setup(s => s.CreateUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await CreateInternalController().CreateUserAsync(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateUser_EmptyId_Returns400()
    {
        var result = await CreateInternalController().CreateUserAsync(Guid.Empty);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateUser_MissingApiKey_ReturnsUnauthorized()
    {
        var result = await CreateInternalController(apiKey: null).CreateUserAsync(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
        _users.Verify(s => s.CreateUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_WrongApiKey_ReturnsUnauthorized()
    {
        var result = await CreateInternalController(apiKey: "wrong").CreateUserAsync(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }


    [Fact]
    public async Task GetCv_Existing_Returns200()
    {
        var userId = Guid.NewGuid();
        var cvId = Guid.NewGuid();
        var cv = TestData.CvDetails();
        _cvs.Setup(s => s.GetCvAsync(userId, cvId, true)).ReturnsAsync(cv);

        var result = await CreateInternalController().GetCvAsync(userId, cvId, includeDeleted: true);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(cv, ok.Value);
    }

    [Fact]
    public async Task GetCv_Missing_Returns404()
    {
        var result = await CreateInternalController().GetCvAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCv_WrongApiKey_ReturnsUnauthorized()
    {
        var result = await CreateInternalController(apiKey: "wrong").GetCvAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result.Result);
    }
}
