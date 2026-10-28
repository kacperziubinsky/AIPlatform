using System.Net;
using Recruitment.DTOs;

namespace Recruitment.Services;

public class UserServiceClient(HttpClient client) : IUserServiceClient
{
    public async Task<CvDto?> GetCvAsync(Guid userId, Guid cvId, CancellationToken ct = default)
    {
        using var response = await client.GetAsync($"internal/users/{userId}/cvs/{cvId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CvDto>(ct);
    }
}
