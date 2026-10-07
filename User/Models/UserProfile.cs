using User.Models.Enums;

namespace User.Models;

public class UserProfile
{
    public Guid Id { get; set; }

    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }

    public List<string> Stack { get; set; } = new();

    public Experience Experience { get; set; }

    public List<CV> CVs { get; set; } = new();
    
}