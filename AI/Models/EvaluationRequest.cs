using System.ComponentModel.DataAnnotations;

namespace Applicant.Models;

public class EvaluationRequest
{
    [Required]
    public EvaluationOffer Offer { get; set; } = new();

    [Required]
    public EvaluationCandidate Candidate { get; set; } = new();
}

public class EvaluationOffer
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Experience { get; set; }
    public List<EvaluationRequirement> Requirements { get; set; } = new();
}

public class EvaluationRequirement
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class EvaluationCandidate
{
    public List<string> Skills { get; set; } = new();
    public List<WorkExperience> WorkHistory { get; set; } = new();
    public List<EducationItem> Education { get; set; } = new();
    public List<LanguageItem> Languages { get; set; } = new();
}
