using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models;
using Recruitment.Models.Enums;

namespace Recruitment.Tests;

public static class TestData
{
    public static ControllerContext ControllerContext(Guid? userId = null)
    {
        var claims = new List<Claim>();

        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            }
        };
    }

    public static CreateApplicationRequest CreateRequest(Guid? offerId = null, Guid? cvId = null) => new()
    {
        OfferId = offerId ?? Guid.NewGuid(),
        CvId = cvId ?? Guid.NewGuid(),
        CoverLetter = "Chętnie dołączę."
    };

    public static ChangeStatusRequest StatusRequest(ApplicationStatus status = ApplicationStatus.InReview) => new()
    {
        Status = status,
        Note = "notatka"
    };

    public static CreateEvaluationRequest EvaluationRequest(int score = 80) => new()
    {
        Score = score,
        Comment = "Dobry kandydat"
    };

    public static OfferRequirementDto Requirement(string name = "C#", int weight = 1, bool required = false) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Weight = weight,
        IsRequired = required
    };

    public static OfferDto Offer(
        Guid? id = null,
        Guid? authorId = null,
        DateTime? endDate = null,
        List<OfferRequirementDto>? requirements = null,
        List<string>? stack = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        AuthorId = authorId ?? Guid.NewGuid(),
        Title = "Backend dev",
        Description = "Opis",
        Company = "ACME",
        Experience = OfferExperience.Junior,
        EndDate = endDate ?? DateTime.UtcNow.AddDays(30),
        Requirements = requirements ?? [],
        Stack = stack ?? []
    };

    public static CvDto Cv(string status = "Completed", string? error = null) => new()
    {
        ParseStatus = status,
        ParseError = error,
        Data = status == "Completed" ? JsonSerializer.SerializeToElement(new { skills = new[] { "C#" } }) : null
    };

    public static AiEvaluationResult AiResult(params (Guid RequirementId, string Level)[] answers) => new()
    {
        Assessments = answers.Select(a => new AiAssessmentDto(a.RequirementId, a.Level, "dowód")).ToList(),
        Summary = "Pasuje."
    };

    public static ApplicationResponse ApplicationResponse() =>
        new(Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Submitted, DateTime.UtcNow);

    public static async Task<JobApplication> SeedApplicationAsync(
        RecruitmentDbContext db,
        Guid? applicantId = null,
        Guid? recruiterId = null,
        Guid? offerId = null,
        ApplicationStatus status = ApplicationStatus.Submitted,
        AiEvaluationStatus aiStatus = AiEvaluationStatus.Pending,
        int? aiScore = null,
        int aiAttempts = 0,
        string? coverLetter = null,
        DateTime? createdAt = null)
    {
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OfferId = offerId ?? Guid.NewGuid(),
            ApplicantId = applicantId ?? Guid.NewGuid(),
            CvId = Guid.NewGuid(),
            RecruiterId = recruiterId ?? Guid.NewGuid(),
            OfferTitle = "Backend dev",
            OfferCompany = "ACME",
            CoverLetter = coverLetter,
            Status = status,
            AiStatus = aiStatus,
            AiScore = aiScore,
            AiAttempts = aiAttempts,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow
        };

        db.Applications.Add(application);
        await db.SaveChangesAsync();
        return application;
    }
}
