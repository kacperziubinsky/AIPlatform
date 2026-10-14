using System.Text.Json.Serialization;

namespace Applicant.Models;

public class ApplicantProfile
{
    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = new();

    [JsonPropertyName("work_history")]
    public List<WorkExperience> WorkHistory { get; set; } = new();

    [JsonPropertyName("education")]
    public List<EducationItem> Education { get; set; } = new();

    [JsonPropertyName("languages")]
    public List<LanguageItem> Languages { get; set; } = new();
}