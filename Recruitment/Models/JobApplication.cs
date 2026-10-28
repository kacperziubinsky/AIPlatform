using Recruitment.Models.Enums;

namespace Recruitment.Models;

public class JobApplication
{
    public Guid Id { get; set; }

    public Guid OfferId { get; set; }
    public Guid ApplicantId { get; set; }
    public Guid CvId { get; set; }

    public Guid? RecruiterId { get; set; }
    public string? OfferTitle { get; set; }

    public string? CoverLetter { get; set; }
    public string? RecruiterNote { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    public AiEvaluationStatus AiStatus { get; set; } = AiEvaluationStatus.Pending;
    public int? AiScore { get; set; }
    public int AiAttempts { get; set; }
    public string? AiError { get; set; }
    public DateTime? AiNextAttemptAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Evaluation> Evaluations { get; set; } = new();
}