using Recruitment.Models;
using Recruitment.Models.Enums;

namespace Recruitment.Services;

/// <summary>
/// Turns the AI's per-requirement match levels into a 0-100 score. The AI never produces the number itself,
/// so the score is deterministic for a given breakdown and the weights stay under the recruiter's control.
/// </summary>
public static class ScoreCalculator
{
    // Each must-have requirement with no evidence at all halves the score.
    public const double MissingRequiredPenalty = 0.5;

    public static int Calculate(IReadOnlyCollection<RequirementAssessment> assessments)
    {
        if (assessments.Count == 0)
            return 0;

        double totalWeight = 0;
        double achieved = 0;
        var missingRequired = 0;

        foreach (var assessment in assessments)
        {
            var weight = Math.Max(assessment.Weight, 1);

            totalWeight += weight;
            achieved += weight * LevelValue(assessment.Level);

            if (assessment.IsRequired && assessment.Level == MatchLevel.None)
                missingRequired++;
        }

        var score = achieved / totalWeight * 100 * Math.Pow(MissingRequiredPenalty, missingRequired);
        return (int)Math.Round(score, MidpointRounding.AwayFromZero);
    }

    public static double LevelValue(MatchLevel level) => level switch
    {
        MatchLevel.Full => 1.0,
        MatchLevel.Partial => 0.6,
        MatchLevel.Weak => 0.25,
        _ => 0.0
    };
}
