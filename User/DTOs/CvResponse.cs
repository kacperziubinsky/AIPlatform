using User.Models.Cv;

namespace User.DTOs;

public record CvResponse
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Location { get; init; }
    public List<string> Skills { get; init; } = new();
    public List<ParsedWorkExperience> WorkHistory { get; init; } = new();
    public List<ParsedEducation> Education { get; init; } = new();
    public List<ParsedLanguage> Languages { get; init; } = new();
}