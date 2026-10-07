using User.Models.Enums;

namespace User.DTOs;

public record UserResponse
{

    public string? Firstname { get; init; }
    public string? Lastname { get; init; }
    public string? Phone { get; init; }

    public List<string> Stack { get; init; } = new();

    public Experience Experience { get; init; }
    
}