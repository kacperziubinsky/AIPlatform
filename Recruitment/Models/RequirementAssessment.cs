using Recruitment.Models.Enums;

namespace Recruitment.Models;

// Snapshot of the requirement at evaluation time, so the score stays explainable after the offer is edited.
public class RequirementAssessment
{
    public Guid RequirementId { get; set; }
    public string Name { get; set; } = "";
    public int Weight { get; set; }
    public bool IsRequired { get; set; }

    public MatchLevel Level { get; set; }
    public string? Evidence { get; set; }
}
