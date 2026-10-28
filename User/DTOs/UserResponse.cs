using User.Models.Enums;

namespace User.DTOs;

public record UserResponse
{
    public Guid Id { get; init; }
    public string? Firstname { get; init; }
    public string? Lastname { get; init; }
    public string? Phone { get; init; }
    public string? City { get; init; }

    public List<string> Stack { get; init; } = new();

    public Experience Experience { get; init; }
    
}