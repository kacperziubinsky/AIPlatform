using Recruitment.Models;

namespace Recruitment.Services;

public interface IAiEvaluationService
{
    /// <summary>
    /// Evaluates a claimed application and updates its AI fields: Completed with a score,
    /// Pending with a retry time, or Failed with an error. The caller saves the changes.
    /// </summary>
    Task EvaluateAsync(JobApplication application, CancellationToken ct = default);
}
