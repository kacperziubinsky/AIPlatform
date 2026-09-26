using Auth.Models.Enums;

namespace Auth.Models;

public class User
{
    public Guid Id { get; set; }
    public  string Email { get; init; }
    public  string Password { get; set; }
    public string? Firstname { get; init; }
    public string? Lastname { get; init; }
    public string? Phone { get; init; }
    public Role Role { get; init; }
    public List<String> Stack { get; set; } = new List<string>();
    public Experience Experience { get; set; }
    public Boolean Active { get; set; }
}