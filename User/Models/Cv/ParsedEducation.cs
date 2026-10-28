using System.Text.Json.Serialization;

namespace User.Models.Cv;

public class ParsedEducation
{
    [JsonPropertyName("degree")]
    public string? Degree { get; set; }

    [JsonPropertyName("school")]
    public string? School { get; set; }

    [JsonPropertyName("field")]
    public string? Field { get; set; }
}