using User.DTOs;

namespace User.Services;

public interface IUserService
{
    Task<bool> CreateUserAsync(Guid id);
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> GetUserAsync(Guid userId);
    Task<UserResponse?> UpdateUserAsync(Guid userId, UpdateUserRequest request);
}