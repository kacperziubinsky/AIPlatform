namespace User.Models;
using User.Models.Cv;

public class ParsedCvData
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<ParsedWorkExperience> WorkHistory { get; set; } = new();
    public List<ParsedEducation> Education { get; set; } = new();
    public List<ParsedLanguage> Languages { get; set; } = new();
}