using Microsoft.EntityFrameworkCore;
using User.Data;
using User.DTOs;
using User.Models;

namespace User.Services;

public class UserService : IUserService
{
    private readonly UserDbContext _context;

    public UserService(UserDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            return false;
        }

        UserProfile account = new UserProfile();
        account.Id = Guid.NewGuid();
        
        _context.Users.Add(account);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserResponse
            {
                Firstname = u.Firstname,
                Lastname = u.Lastname,
                Phone = u.Phone,
                Stack = u.Stack,
                Experience = u.Experience
            })
            .ToListAsync();
    }
    
    public async Task<UserResponse?> GetUserAsync(Guid userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserResponse
            {
                Firstname = u.Firstname,
                Lastname = u.Lastname,
                Phone = u.Phone,
                Stack = u.Stack,
                Experience = u.Experience
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            user = new UserProfile { Id = userId };
            _context.Users.Add(user);
        }

        user.Firstname = request.Firstname?.Trim();
        user.Lastname = request.Lastname?.Trim();
        user.Phone = request.Phone?.Trim();
        user.City = request.City?.Trim();
        user.Stack = request.Stack;
        user.Experience = request.Experience;

        await _context.SaveChangesAsync();

        return await GetUserAsync(userId);
    }
}