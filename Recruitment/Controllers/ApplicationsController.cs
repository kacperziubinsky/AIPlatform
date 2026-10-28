using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruitment.DTOs;
using Recruitment.Extensions;
using Recruitment.Services;

namespace Recruitment.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }


    [HttpPost("applications")]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> CreateAsync(CreateApplicationRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            var application = await _applicationService.CreateAsync(userId.Value, request);
            return application is null
                ? Conflict("You have already applied for this offer.")
                : StatusCode(201, application);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("applications/me")]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> GetMineAsync()
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        return Ok(await _applicationService.GetMyApplicationsAsync(userId.Value));
    }

    [HttpDelete("applications/{id:guid}")]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> WithdrawAsync(Guid id)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            var withdrawn = await _applicationService.WithdrawAsync(userId.Value, id);
            return withdrawn ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }


    [HttpGet("offers/{offerId:guid}/applications")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> GetForOfferAsync(Guid offerId)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        return Ok(await _applicationService.GetForOfferAsync(userId.Value, offerId));
    }

    [HttpGet("applications/{id:guid}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var application = await _applicationService.GetByIdAsync(userId.Value, id);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPatch("applications/{id:guid}/status")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> ChangeStatusAsync(Guid id, ChangeStatusRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            var changed = await _applicationService.ChangeStatusAsync(userId.Value, id, request);
            return changed ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("applications/{id:guid}/ai-evaluation")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> RequestAiEvaluationAsync(Guid id)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var requested = await _applicationService.RequestAiEvaluationAsync(userId.Value, id);
        return requested ? Accepted() : NotFound();
    }

    [HttpPost("applications/{id:guid}/evaluations")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> AddEvaluationAsync(Guid id, CreateEvaluationRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var evaluation = await _applicationService.AddEvaluationAsync(userId.Value, id, request);
        return evaluation is null ? NotFound() : StatusCode(201, evaluation);
    }
}
