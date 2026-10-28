using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.DTOs;
using User.Extensions;
using User.Services;

namespace User.Controllers;

[ApiController]
[Route("api/users/me/cvs")]
[Authorize(Roles = "Applicant")]
public class CVController : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly ICvService _cvService;

    public CVController(ICvService cvService)
    {
        _cvService = cvService;
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
    public async Task<IActionResult> UploadAsync(IFormFile? file)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        if (file.Length > MaxFileSize)
            return BadRequest("File is too large (max 5 MB).");

        if (!await HasValidSignatureAsync(file))
            return BadRequest("Only PDF or DOCX files are allowed.");

        var uploaded = await _cvService.UploadCvAsync(file, userId.Value);
        return uploaded
            ? StatusCode(201)
            : BadRequest("Invalid file. Allowed: PDF or DOCX, max 5 MB.");
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CvItemResponse>>> GetAllAsync()
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        return Ok(await _cvService.GetCvsAsync(userId.Value));
    }

    [HttpGet("{cvId:guid}")]
    public async Task<ActionResult<CvDetailsResponse>> GetAsync(Guid cvId)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var cv = await _cvService.GetCvAsync(userId.Value, cvId);
        return cv is null ? NotFound() : Ok(cv);
    }

    [HttpDelete("{cvId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid cvId)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var deleted = await _cvService.DeleteCvAsync(userId.Value, cvId);
        return deleted ? NoContent() : NotFound();
    }

    private static async Task<bool> HasValidSignatureAsync(IFormFile file)
    {
        var header = new byte[4];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header);

        if (read < 4)
            return false;

        var isPdf = header[0] == '%' && header[1] == 'P' && header[2] == 'D' && header[3] == 'F';
        var isZip = header[0] == 'P' && header[1] == 'K' && header[2] == 3 && header[3] == 4;

        return isPdf || isZip;
    }
}