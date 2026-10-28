using System.Text.Json;
using User.Models;

namespace User.Services;

public class CvParserClient(HttpClient client) : ICvParserClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public async Task<(ParsedCvData Data, string RawJson)> ParseAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(content), "file", fileName);

        using var response = await client.PostAsync("api/applicant", form, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"AI service returned {(int)response.StatusCode}: {body}");

        var data = JsonSerializer.Deserialize<ParsedCvData>(body, JsonOptions)
                   ?? throw new InvalidOperationException("AI service returned an empty result.");

        data.Skills ??= new();
        data.WorkHistory ??= new();
        data.Education ??= new();
        data.Languages ??= new();

        return (data, body);
    }
}
