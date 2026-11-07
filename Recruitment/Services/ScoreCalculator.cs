using Recruitment.Models;
using Recruitment.Models.Enums;

namespace Recruitment.Services;

public static class ScoreCalculator
{
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
