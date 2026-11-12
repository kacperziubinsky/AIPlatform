using System.Net;
using Recruitment.DTOs;

namespace Recruitment.Services;

public class UserServiceClient(HttpClient client) : IUserServiceClient
{
    public async Task<CvDto?> GetCvAsync(Guid userId, Guid cvId, bool includeDeleted = false, CancellationToken ct = default)
    {
        var url = $"internal/users/{userId}/cvs/{cvId}";
        if (includeDeleted)
            url += "?includeDeleted=true";

        using var response = await client.GetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CvDto>(ct);
    }
}
