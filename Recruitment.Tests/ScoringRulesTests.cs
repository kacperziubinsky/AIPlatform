using Recruitment.Models;
using Recruitment.Models.Enums;
using Recruitment.Services;

namespace Recruitment.Tests;

public class ScoringRulesTests
{
    private static RequirementAssessment Assessment(MatchLevel level, int weight = 1, bool required = false) => new()
    {
        RequirementId = Guid.NewGuid(),
        Name = "req",
        Level = level,
        Weight = weight,
        IsRequired = required
    };


    [Fact]
    public void Score_NoAssessments_IsZero() =>
        Assert.Equal(0, ScoreCalculator.Calculate([]));

    [Theory]
    [InlineData(MatchLevel.Full, 100)]
    [InlineData(MatchLevel.Partial, 60)]
    [InlineData(MatchLevel.Weak, 25)]
    [InlineData(MatchLevel.None, 0)]
    public void Score_SingleRequirement_MatchesLevelValue(MatchLevel level, int expected) =>
        Assert.Equal(expected, ScoreCalculator.Calculate([Assessment(level)]));

    [Fact]
    public void Score_UsesWeights() =>
        Assert.Equal(75, ScoreCalculator.Calculate([
            Assessment(MatchLevel.Full, weight: 3),
            Assessment(MatchLevel.None, weight: 1)
        ]));

    [Fact]
    public void Score_WeightBelowOne_CountsAsOne() =>
        Assert.Equal(50, ScoreCalculator.Calculate([
            Assessment(MatchLevel.Full, weight: 0),
            Assessment(MatchLevel.None, weight: 1)
        ]));

    [Fact]
    public void Score_EachMissingRequiredHalvesScore() =>
        // 50% achieved, two required misses: 50 * 0.5 * 0.5 = 12.5, rounded away from zero
        Assert.Equal(13, ScoreCalculator.Calculate([
            Assessment(MatchLevel.Full, weight: 2),
            Assessment(MatchLevel.None, required: true),
            Assessment(MatchLevel.None, required: true)
        ]));


    [Theory]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.InReview, true)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Hired, true)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.Rejected, true)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.InReview, false)]
    [InlineData(ApplicationStatus.InReview, ApplicationStatus.InReview, false)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Withdrawn, false)]
    [InlineData(ApplicationStatus.Hired, ApplicationStatus.Rejected, false)]
    [InlineData(ApplicationStatus.Withdrawn, ApplicationStatus.InReview, false)]
    public void Status_RecruiterTransitions(ApplicationStatus from, ApplicationStatus to, bool allowed) =>
        Assert.Equal(allowed, ApplicationStatusRules.CanRecruiterChange(from, to));
}
