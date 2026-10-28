namespace Auth.Services;

public class UserServiceClient(HttpClient client) : IUserServiceClient
{
    public async Task CreateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await client.PostAsync($"internal/users/{userId}", null, ct);
        response.EnsureSuccessStatusCode();
    }
}