namespace Applicant.Models;

public class EvaluationResponse
{
    public List<RequirementAssessment> Assessments { get; set; } = new();
    public string? Summary { get; set; }
}

public class RequirementAssessment
{
    public Guid RequirementId { get; set; }

    public string Level { get; set; } = string.Empty;

    public string? Evidence { get; set; }
}

public class LlmEvaluation
{
    public List<LlmAssessment>? Requirements { get; set; }
    public string? Summary { get; set; }
}

public class LlmAssessment
{
    public string? Id { get; set; }
    public string? Level { get; set; }
    public string? Evidence { get; set; }
}
