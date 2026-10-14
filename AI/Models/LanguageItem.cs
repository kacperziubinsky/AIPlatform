using System.Text.Json.Serialization;

namespace Applicant.Models;

public class LanguageItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public string? Level { get; set; }
}