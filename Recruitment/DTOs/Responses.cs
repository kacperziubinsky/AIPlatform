using Recruitment.Models.Enums;

namespace Recruitment.DTOs;

public record ApplicationResponse(
    Guid Id,
    Guid OfferId,
    ApplicationStatus Status,
    DateTime CreatedAt);

public record MyApplicationResponse(
    Guid Id,
    Guid OfferId,
    string? OfferTitle,
    string? Company,
    ApplicationStatus Status,
    DateTime AppliedAt,
    DateTime UpdatedAt);

public record RecruiterApplicationResponse(
    Guid Id,
    Guid OfferId,
    Guid ApplicantId,
    Guid CvId,
    string? CoverLetter,
    ApplicationStatus Status,
    AiEvaluationStatus AiStatus,
    int? AiScore,
    string? AiError,
    List<EvaluationResponse> Evaluations,
    DateTime CreatedAt);

public record EvaluationResponse(
    Guid Id,
    int Score,
    string? Comment,
    EvaluationSource Source,
    List<RequirementAssessmentResponse> Breakdown,
    DateTime CreatedAt);

public record RequirementAssessmentResponse(
    Guid RequirementId,
    string Name,
    int Weight,
    bool IsRequired,
    MatchLevel Level,
    string? Evidence);