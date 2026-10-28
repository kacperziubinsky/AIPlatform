using Recruitment.Models.Enums;

namespace Recruitment.Models;

public class Evaluation
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    public int Score { get; set; }
    public string? Comment { get; set; }

    public EvaluationSource Source { get; set; }
    public Guid? EvaluatedBy { get; set; }

    // Per-requirement result; filled for AI evaluations only.
    public List<RequirementAssessment> Breakdown { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public JobApplication Application { get; set; } = null!;
}