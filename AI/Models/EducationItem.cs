using System.Text.Json.Serialization;

namespace Applicant.Models;

public class EducationItem
{
    [JsonPropertyName("degree")]
    public string? Degree { get; set; }

    [JsonPropertyName("school")]
    public string? School { get; set; }

    [JsonPropertyName("field")]
    public string? Field { get; set; }
}