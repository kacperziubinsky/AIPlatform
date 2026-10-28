using Recruitment.Models.Enums;

namespace Recruitment.Services;

/// <summary>
/// Recruiter pipeline: Submitted → InReview → Interview → Offered → Hired. Steps may be skipped forward,
/// any open application may be Rejected, and Hired/Rejected/Withdrawn are final.
/// </summary>
public static class ApplicationStatusRules
{
    private static readonly ApplicationStatus[] Pipeline =
    [
        ApplicationStatus.Submitted,
        ApplicationStatus.InReview,
        ApplicationStatus.Interview,
        ApplicationStatus.Offered,
        ApplicationStatus.Hired
    ];

    public static bool IsFinal(ApplicationStatus status) =>
        status is ApplicationStatus.Hired or ApplicationStatus.Rejected or ApplicationStatus.Withdrawn;

    public static bool CanRecruiterChange(ApplicationStatus from, ApplicationStatus to)
    {
        if (IsFinal(from))
            return false;

        if (to == ApplicationStatus.Rejected)
            return true;

        var fromIndex = Array.IndexOf(Pipeline, from);
        var toIndex = Array.IndexOf(Pipeline, to);

        // Withdrawn isn't in the pipeline, so only the applicant can set it.
        return toIndex > fromIndex;
    }
}
