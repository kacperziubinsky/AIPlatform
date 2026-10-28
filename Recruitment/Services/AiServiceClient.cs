using Recruitment.DTOs;

namespace Recruitment.Services;

public class AiServiceClient(HttpClient client) : IAiServiceClient
{
    public async Task<AiEvaluationResult> EvaluateAsync(AiEvaluationRequest request, CancellationToken ct = default)
    {
        using var response = await client.PostAsJsonAsync("api/evaluations", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"AI service returned {(int)response.StatusCode}: {body}", null, response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<AiEvaluationResult>(ct)
               ?? throw new InvalidOperationException("AI service returned an empty result.");
    }
}
