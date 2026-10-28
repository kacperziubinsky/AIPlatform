using System.Text.Json.Serialization;

namespace User.Models.Cv;

public class ParsedLanguage
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public string? Level { get; set; }
}