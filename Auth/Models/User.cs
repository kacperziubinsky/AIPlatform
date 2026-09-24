namespace Auth.Models;

public class User
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? Firstname { get; init; }
    public string? Lastname { get; init; }
    public string Phone { get; init; }
}