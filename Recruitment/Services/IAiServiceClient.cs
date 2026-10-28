using Recruitment.DTOs;

namespace Recruitment.Services;

public interface IAiServiceClient
{
    Task<AiEvaluationResult> EvaluateAsync(AiEvaluationRequest request, CancellationToken ct = default);
}
