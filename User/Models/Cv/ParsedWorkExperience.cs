using System.Text.Json.Serialization;

namespace User.Models.Cv;

public class ParsedWorkExperience
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }
}