using Applicant.Controllers;
using Applicant.Models;
using Microsoft.AspNetCore.Mvc;

namespace AI.Tests;

public class ApplicantControllerTests
{
    private readonly StubHttpHandler _http = new();

    private ApplicantController CreateApplicantController() => new(TestData.LlmClient(_http));


    [Fact]
    public async Task Post_ValidPdf_Returns200WithProfile()
    {
        _http.Returns(TestData.LlmReply(new { full_name = "Jan Kowalski", skills = new[] { "C#" } }));

        var result = await CreateApplicantController().Post(TestData.File(TestData.Pdf("Jan Kowalski, C# developer")));

        var profile = Assert.IsType<ApplicantProfile>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("Jan Kowalski", profile.FullName);
    }

    [Fact]
    public async Task Post_ValidPdf_SendsExtractedTextToLlm()
    {
        _http.Returns(TestData.LlmReply("{}"));

        await CreateApplicantController().Post(TestData.File(TestData.Pdf("Jan Kowalski, C# developer")));

        Assert.Contains("Jan Kowalski", Assert.Single(_http.RequestBodies));
    }

    [Fact]
    public async Task Post_NoFile_Returns400() =>
        await AssertBadRequest(null);

    [Fact]
    public async Task Post_EmptyFile_Returns400() =>
        await AssertBadRequest(TestData.File([]));

    [Fact]
    public async Task Post_NotPdfExtension_Returns400() =>
        await AssertBadRequest(TestData.File(TestData.Pdf(), "cv.docx"));

    [Fact]
    public async Task Post_CorruptPdf_Returns400() =>
        await AssertBadRequest(TestData.File("not a pdf at all"u8.ToArray()));

    private async Task AssertBadRequest(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        var result = await CreateApplicantController().Post(file);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(_http.RequestBodies);
    }

    [Fact]
    public async Task Post_PdfWithoutText_Returns422()
    {
        var result = await CreateApplicantController().Post(TestData.File(TestData.Pdf(text: null)));

        Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        Assert.Empty(_http.RequestBodies);
    }

    [Fact]
    public async Task Post_UnparsableLlmReply_Returns500()
    {
        _http.Returns(TestData.LlmReply("nie json"));

        var result = await CreateApplicantController().Post(TestData.File(TestData.Pdf()));

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, obj.StatusCode);
    }
}
