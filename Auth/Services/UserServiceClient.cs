namespace Auth.Services;

public class UserServiceClient(HttpClient client) : IUserServiceClient
{
    public async Task CreateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await client.PutAsJsonAsync("/api/users", userId, ct);
        response.EnsureSuccessStatusCode();
    }
}