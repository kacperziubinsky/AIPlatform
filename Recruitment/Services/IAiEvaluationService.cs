using Recruitment.Models;

namespace Recruitment.Services;

public interface IAiEvaluationService
{
    Task EvaluateAsync(JobApplication application, CancellationToken ct = default);
}
