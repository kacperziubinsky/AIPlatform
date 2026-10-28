using Microsoft.EntityFrameworkCore;
using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models;
using Recruitment.Models.Enums;

namespace Recruitment.Services;

public class ApplicationService : IApplicationService
{
    private readonly RecruitmentDbContext _context;
    private readonly IOfferServiceClient _offers;
    private readonly IUserServiceClient _users;

    public ApplicationService(RecruitmentDbContext context, IOfferServiceClient offers, IUserServiceClient users)
    {
        _context = context;
        _offers = offers;
        _users = users;
    }

    public async Task<ApplicationResponse?> CreateAsync(Guid applicantId, CreateApplicationRequest request)
    {
        var alreadyApplied = await _context.Applications
            .AnyAsync(a => a.OfferId == request.OfferId && a.ApplicantId == applicantId);

        if (alreadyApplied)
            return null;

        var offer = await _offers.GetOfferAsync(request.OfferId)
                    ?? throw new KeyNotFoundException("Offer not found.");

        if (offer.EndDate <= DateTime.UtcNow)
            throw new InvalidOperationException("This offer is no longer accepting applications.");

        // The User service only returns CVs owned by the given user.
        _ = await _users.GetCvAsync(applicantId, request.CvId)
            ?? throw new KeyNotFoundException("CV not found.");

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OfferId = request.OfferId,
            ApplicantId = applicantId,
            CvId = request.CvId,
            RecruiterId = offer.AuthorId,
            OfferTitle = offer.Title,
            CoverLetter = request.CoverLetter,
            Status = ApplicationStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        return new ApplicationResponse(application.Id, application.OfferId, application.Status, application.CreatedAt);
    }

    public async Task<List<ApplicationResponse>> GetMyApplicationsAsync(Guid applicantId)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.ApplicantId == applicantId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ApplicationResponse(a.Id, a.OfferId, a.Status, a.CreatedAt))
            .ToListAsync();
    }

    public async Task<bool> WithdrawAsync(Guid applicantId, Guid applicationId)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.ApplicantId == applicantId);

        if (application is null)
            return false;

        if (ApplicationStatusRules.IsFinal(application.Status))
            throw new InvalidOperationException($"Application is already {application.Status} and can't be withdrawn.");

        application.Status = ApplicationStatus.Withdrawn;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<List<RecruiterApplicationResponse>> GetForOfferAsync(Guid recruiterId, Guid offerId)
    {
        // PostgreSQL sorts NULLs first in descending order, so unscored applications are put last explicitly.
        var applications = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Evaluations)
            .Where(a => a.OfferId == offerId && a.RecruiterId == recruiterId)
            .OrderByDescending(a => a.AiScore.HasValue)
            .ThenByDescending(a => a.AiScore)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        return applications.Select(ToRecruiterResponse).ToList();
    }

    public async Task<RecruiterApplicationResponse?> GetByIdAsync(Guid recruiterId, Guid applicationId)
    {
        var application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Evaluations)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.RecruiterId == recruiterId);

        return application is null ? null : ToRecruiterResponse(application);
    }

    public async Task<bool> ChangeStatusAsync(Guid recruiterId, Guid applicationId, ChangeStatusRequest request)
    {
        var application = await FindOwnedAsync(recruiterId, applicationId);

        if (application is null)
            return false;

        if (!ApplicationStatusRules.CanRecruiterChange(application.Status, request.Status))
            throw new InvalidOperationException($"Can't change status from {application.Status} to {request.Status}.");

        application.Status = request.Status;
        application.RecruiterNote = request.Note;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EvaluationResponse?> AddEvaluationAsync(Guid recruiterId, Guid applicationId, CreateEvaluationRequest request)
    {
        var exists = await _context.Applications.AnyAsync(a => a.Id == applicationId && a.RecruiterId == recruiterId);

        if (!exists)
            return null;

        var evaluation = new Evaluation
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Score = request.Score,
            Comment = request.Comment,
            Source = EvaluationSource.Recruiter,
            EvaluatedBy = recruiterId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Evaluations.Add(evaluation);
        await _context.SaveChangesAsync();

        return ToEvaluationResponse(evaluation);
    }

    public async Task<bool> RequestAiEvaluationAsync(Guid recruiterId, Guid applicationId)
    {
        var application = await FindOwnedAsync(recruiterId, applicationId);

        if (application is null)
            return false;

        // Previous AI evaluations are kept as history; the worker adds a new one and updates AiScore.
        application.AiStatus = AiEvaluationStatus.Pending;
        application.AiAttempts = 0;
        application.AiError = null;
        application.AiNextAttemptAt = null;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Applications for another recruiter's offer are reported as not found.
    private Task<JobApplication?> FindOwnedAsync(Guid recruiterId, Guid applicationId) =>
        _context.Applications.FirstOrDefaultAsync(a => a.Id == applicationId && a.RecruiterId == recruiterId);

    private static RecruiterApplicationResponse ToRecruiterResponse(JobApplication a) =>
        new(
            a.Id,
            a.OfferId,
            a.ApplicantId,
            a.CvId,
            a.CoverLetter,
            a.Status,
            a.AiStatus,
            a.AiScore,
            a.AiError,
            a.Evaluations
                .OrderByDescending(e => e.CreatedAt)
                .Select(ToEvaluationResponse)
                .ToList(),
            a.CreatedAt);

    private static EvaluationResponse ToEvaluationResponse(Evaluation e) =>
        new(
            e.Id,
            e.Score,
            e.Comment,
            e.Source,
            e.Breakdown
                .Select(b => new RequirementAssessmentResponse(b.RequirementId, b.Name, b.Weight, b.IsRequired, b.Level, b.Evidence))
                .ToList(),
            e.CreatedAt);
}
