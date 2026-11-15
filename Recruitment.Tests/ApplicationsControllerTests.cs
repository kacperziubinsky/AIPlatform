using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Recruitment.DTOs;
using Recruitment.Services;

namespace Recruitment.Tests;

public class ApplicationsControllerTests
{
    private readonly Mock<IApplicationService> _service = new();

    private Controllers.ApplicationsController CreateApplicationsController(Guid? userId = null) =>
        new(_service.Object) { ControllerContext = TestData.ControllerContext(userId) };


    [Fact]
    public async Task Create_Valid_Returns201()
    {
        var userId = Guid.NewGuid();
        var response = TestData.ApplicationResponse();
        _service.Setup(s => s.CreateAsync(userId, It.IsAny<CreateApplicationRequest>())).ReturnsAsync(response);

        var result = await CreateApplicationsController(userId).CreateAsync(TestData.CreateRequest());

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task Create_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateApplicationsController(userId: null).CreateAsync(TestData.CreateRequest());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_AlreadyApplied_Returns409()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateApplicationRequest>()))
                .ReturnsAsync((ApplicationResponse?)null);

        var result = await CreateApplicationsController(Guid.NewGuid()).CreateAsync(TestData.CreateRequest());

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Create_OfferOrCvMissing_Returns404()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateApplicationRequest>()))
                .ThrowsAsync(new KeyNotFoundException("Offer not found."));

        var result = await CreateApplicationsController(Guid.NewGuid()).CreateAsync(TestData.CreateRequest());

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Offer not found.", notFound.Value);
    }

    [Fact]
    public async Task Create_OfferClosed_Returns400()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateApplicationRequest>()))
                .ThrowsAsync(new InvalidOperationException("closed"));

        var result = await CreateApplicationsController(Guid.NewGuid()).CreateAsync(TestData.CreateRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public async Task GetMine_Returns200()
    {
        var userId = Guid.NewGuid();
        var applications = new List<MyApplicationResponse>();
        _service.Setup(s => s.GetMyApplicationsAsync(userId)).ReturnsAsync(applications);

        var result = await CreateApplicationsController(userId).GetMineAsync();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(applications, ok.Value);
    }

    [Fact]
    public async Task GetMine_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateApplicationsController(userId: null).GetMineAsync();

        Assert.IsType<UnauthorizedResult>(result);
    }


    [Fact]
    public async Task Withdraw_Valid_Returns204()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _service.Setup(s => s.WithdrawAsync(userId, id)).ReturnsAsync(true);

        var result = await CreateApplicationsController(userId).WithdrawAsync(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Withdraw_Missing_Returns404()
    {
        var result = await CreateApplicationsController(Guid.NewGuid()).WithdrawAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Withdraw_FinalStatus_Returns409()
    {
        _service.Setup(s => s.WithdrawAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new InvalidOperationException("final"));

        var result = await CreateApplicationsController(Guid.NewGuid()).WithdrawAsync(Guid.NewGuid());

        Assert.IsType<ConflictObjectResult>(result);
    }


    [Fact]
    public async Task GetForOffer_Returns200()
    {
        var userId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var applications = new List<RecruiterApplicationResponse>();
        _service.Setup(s => s.GetForOfferAsync(userId, offerId)).ReturnsAsync(applications);

        var result = await CreateApplicationsController(userId).GetForOfferAsync(offerId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(applications, ok.Value);
    }

    [Fact]
    public async Task GetById_Missing_Returns404()
    {
        var result = await CreateApplicationsController(Guid.NewGuid()).GetByIdAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task ChangeStatus_Valid_Returns204()
    {
        _service.Setup(s => s.ChangeStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ChangeStatusRequest>()))
                .ReturnsAsync(true);

        var result = await CreateApplicationsController(Guid.NewGuid())
            .ChangeStatusAsync(Guid.NewGuid(), TestData.StatusRequest());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ChangeStatus_Missing_Returns404()
    {
        var result = await CreateApplicationsController(Guid.NewGuid())
            .ChangeStatusAsync(Guid.NewGuid(), TestData.StatusRequest());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ChangeStatus_NotAllowed_Returns409()
    {
        _service.Setup(s => s.ChangeStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ChangeStatusRequest>()))
                .ThrowsAsync(new InvalidOperationException("no"));

        var result = await CreateApplicationsController(Guid.NewGuid())
            .ChangeStatusAsync(Guid.NewGuid(), TestData.StatusRequest());

        Assert.IsType<ConflictObjectResult>(result);
    }


    [Fact]
    public async Task RequestAiEvaluation_Valid_Returns202()
    {
        _service.Setup(s => s.RequestAiEvaluationAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await CreateApplicationsController(Guid.NewGuid()).RequestAiEvaluationAsync(Guid.NewGuid());

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task RequestAiEvaluation_Missing_Returns404()
    {
        var result = await CreateApplicationsController(Guid.NewGuid()).RequestAiEvaluationAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task AddEvaluation_Valid_Returns201()
    {
        _service.Setup(s => s.AddEvaluationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateEvaluationRequest>()))
                .ReturnsAsync(new EvaluationResponse(Guid.NewGuid(), 80, null, default, [], DateTime.UtcNow));

        var result = await CreateApplicationsController(Guid.NewGuid())
            .AddEvaluationAsync(Guid.NewGuid(), TestData.EvaluationRequest());

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
    }

    [Fact]
    public async Task AddEvaluation_Missing_Returns404()
    {
        var result = await CreateApplicationsController(Guid.NewGuid())
            .AddEvaluationAsync(Guid.NewGuid(), TestData.EvaluationRequest());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddEvaluation_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateApplicationsController(userId: null)
            .AddEvaluationAsync(Guid.NewGuid(), TestData.EvaluationRequest());

        Assert.IsType<UnauthorizedResult>(result);
    }
}
