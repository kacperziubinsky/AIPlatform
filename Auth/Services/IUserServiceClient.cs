namespace Auth.Services;

public interface IUserServiceClient
{
    public Task CreateUserAsync (Guid userId, CancellationToken ct = default);
    
}