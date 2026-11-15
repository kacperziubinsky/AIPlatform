using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using User.DTOs;
using User.Services;

namespace User.Tests;

public class CVControllerTests
{
    private readonly Mock<ICvService> _service = new();

    public CVControllerTests()
    {
        _service.Setup(s => s.UploadCvAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>())).ReturnsAsync(true);
    }

    private Controllers.CVController CreateCvController(Guid? userId = null) =>
        new(_service.Object) { ControllerContext = TestData.ControllerContext(userId) };


    [Fact]
    public async Task Upload_Pdf_Returns201()
    {
        var userId = Guid.NewGuid();

        var result = await CreateCvController(userId).UploadAsync(TestData.File());

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status201Created, status.StatusCode);
        _service.Verify(s => s.UploadCvAsync(It.IsAny<IFormFile>(), userId), Times.Once);
    }

    [Fact]
    public async Task Upload_Docx_Returns201()
    {
        var result = await CreateCvController(Guid.NewGuid())
            .UploadAsync(TestData.File("cv.docx", TestData.DocxBytes));

        Assert.IsType<StatusCodeResult>(result);
    }

    [Fact]
    public async Task Upload_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateCvController(userId: null).UploadAsync(TestData.File());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Upload_NoFile_Returns400() =>
        await AssertUploadRejected(null);

    [Fact]
    public async Task Upload_TooLarge_Returns400() =>
        await AssertUploadRejected(TestData.File(content: new byte[5 * 1024 * 1024 + 1]));

    [Fact]
    public async Task Upload_NotPdfOrDocxContent_Returns400() =>
        await AssertUploadRejected(TestData.File(content: "just text"u8.ToArray()));

    [Fact]
    public async Task Upload_ServiceRejects_Returns400()
    {
        _service.Setup(s => s.UploadCvAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await CreateCvController(Guid.NewGuid()).UploadAsync(TestData.File("cv.txt"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private async Task AssertUploadRejected(IFormFile? file)
    {
        var result = await CreateCvController(Guid.NewGuid()).UploadAsync(file);

        Assert.IsType<BadRequestObjectResult>(result);
        _service.Verify(s => s.UploadCvAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()), Times.Never);
    }


    [Fact]
    public async Task GetAll_Returns200()
    {
        var userId = Guid.NewGuid();
        var cvs = new List<CvItemResponse> { new() { Id = Guid.NewGuid(), FileName = "cv.pdf" } };
        _service.Setup(s => s.GetCvsAsync(userId)).ReturnsAsync(cvs);

        var result = await CreateCvController(userId).GetAllAsync();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(cvs, ok.Value);
    }

    [Fact]
    public async Task GetAll_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateCvController(userId: null).GetAllAsync();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }


    [Fact]
    public async Task Get_Existing_Returns200()
    {
        var userId = Guid.NewGuid();
        var cvId = Guid.NewGuid();
        var cv = TestData.CvDetails();
        _service.Setup(s => s.GetCvAsync(userId, cvId, false)).ReturnsAsync(cv);

        var result = await CreateCvController(userId).GetAsync(cvId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(cv, ok.Value);
    }

    [Fact]
    public async Task Get_Missing_Returns404()
    {
        var result = await CreateCvController(Guid.NewGuid()).GetAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Get_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateCvController(userId: null).GetAsync(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result.Result);
    }


    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        var userId = Guid.NewGuid();
        var cvId = Guid.NewGuid();
        _service.Setup(s => s.DeleteCvAsync(userId, cvId)).ReturnsAsync(true);

        var result = await CreateCvController(userId).DeleteAsync(cvId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_Missing_Returns404()
    {
        var result = await CreateCvController(Guid.NewGuid()).DeleteAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateCvController(userId: null).DeleteAsync(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(result);
    }
}
