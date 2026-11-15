using Applicant.Controllers;
using Applicant.Models;
using Microsoft.AspNetCore.Mvc;

namespace AI.Tests;

public class EvaluationControllerTests
{
    private readonly StubHttpHandler _http = new();

    private EvaluationController CreateEvaluationController() => new(TestData.LlmClient(_http));

    private void LlmAnswers(params object[] assessments) =>
        _http.Returns(TestData.LlmReply(new { requirements = assessments, summary = "  Dobry kandydat.  " }));


    [Fact]
    public async Task Post_Valid_Returns200WithAssessmentPerRequirement()
    {
        var request = TestData.EvaluationRequest("C#", "SQL");
        LlmAnswers(TestData.Assessment("R1", "full"), TestData.Assessment("R2", "partial"));

        var result = await CreateEvaluationController().Post(request, CancellationToken.None);

        var response = Assert.IsType<EvaluationResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(request.Offer.Requirements.Select(r => r.Id), response.Assessments.Select(a => a.RequirementId));
        Assert.Equal(["full", "partial"], response.Assessments.Select(a => a.Level));
        Assert.Equal("Dobry kandydat.", response.Summary);
    }

    [Fact]
    public async Task Post_IdsAndLevels_AreMatchedCaseInsensitively()
    {
        LlmAnswers(TestData.Assessment(" r1 ", " FULL "));

        var result = await CreateEvaluationController().Post(TestData.EvaluationRequest("C#"), CancellationToken.None);

        var response = Assert.IsType<EvaluationResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("full", Assert.Single(response.Assessments).Level);
    }

    [Fact]
    public async Task Post_LevelWithoutEvidence_BecomesNone()
    {
        LlmAnswers(TestData.Assessment("R1", "full", evidence: " "));

        var result = await CreateEvaluationController().Post(TestData.EvaluationRequest("C#"), CancellationToken.None);

        var assessment = Assert.Single(Assert.IsType<EvaluationResponse>(Assert.IsType<OkObjectResult>(result.Result).Value).Assessments);
        Assert.Equal("none", assessment.Level);
        Assert.Null(assessment.Evidence);
    }

    [Fact]
    public async Task Post_LevelNone_DropsEvidence()
    {
        LlmAnswers(TestData.Assessment("R1", "none", evidence: "brak"));

        var result = await CreateEvaluationController().Post(TestData.EvaluationRequest("C#"), CancellationToken.None);

        var assessment = Assert.Single(Assert.IsType<EvaluationResponse>(Assert.IsType<OkObjectResult>(result.Result).Value).Assessments);
        Assert.Null(assessment.Evidence);
    }

    [Fact]
    public async Task Post_NoRequirements_Returns400WithoutCallingLlm()
    {
        var result = await CreateEvaluationController().Post(TestData.EvaluationRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(_http.RequestBodies);
    }

    [Fact]
    public async Task Post_UnparsableLlmReply_Returns502() =>
        await AssertBadGateway(TestData.LlmReply("nie json"));

    [Fact]
    public async Task Post_RequirementMissingInReply_Returns502() =>
        await AssertBadGateway(TestData.LlmReply(new { requirements = new[] { TestData.Assessment("R1", "full") } }));

    [Fact]
    public async Task Post_UnknownLevel_Returns502() =>
        await AssertBadGateway(TestData.LlmReply(new
        {
            requirements = new[] { TestData.Assessment("R1", "excellent"), TestData.Assessment("R2", "full") }
        }));

    private async Task AssertBadGateway(HttpResponseMessage llmReply)
    {
        _http.Returns(llmReply);

        var result = await CreateEvaluationController().Post(TestData.EvaluationRequest("C#", "SQL"), CancellationToken.None);

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(502, obj.StatusCode);
    }
}
