using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace Applicant.Services;

public class LlmClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly ConcurrentDictionary<string, bool> ModelsWithoutReasoning = new();

    private readonly HttpClient _client;
    private readonly string _url;
    private readonly string _model;
    private readonly string _reasoning;
    private readonly double _temperature;

    public LlmClient(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _url = configuration["Llm:Url"] ?? "http://localhost:1234/api/v1/chat";
        _model = configuration["Llm:Model"] ?? "qwen2.5-7b-instruct";
        _reasoning = configuration["Llm:Reasoning"] ?? "off";
        _temperature = configuration.GetValue("Llm:Temperature", 0.0);
    }
    
    public async Task<T?> CompleteJsonAsync<T>(string systemPrompt, string input, CancellationToken ct = default)
        where T : class
    {
        var sendReasoning = !string.IsNullOrWhiteSpace(_reasoning) && !ModelsWithoutReasoning.ContainsKey(_model);

        var response = await SendAsync(systemPrompt, input, sendReasoning, ct);

        if (sendReasoning && response.StatusCode == HttpStatusCode.BadRequest
                          && (await response.Content.ReadAsStringAsync(ct)).Contains("\"reasoning\""))
        {
            response.Dispose();
            ModelsWithoutReasoning.TryAdd(_model, true);
            response = await SendAsync(systemPrompt, input, sendReasoning: false, ct);
        }

        using var _ = response;
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        var rawJson = ExtractContentFromAiOutput(doc);

        if (string.IsNullOrWhiteSpace(rawJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(CleanJsonOutput(rawJson), JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private Task<HttpResponseMessage> SendAsync(string systemPrompt, string input, bool sendReasoning, CancellationToken ct)
    {
        var request = new Dictionary<string, object>
        {
            ["model"] = _model,
            ["system_prompt"] = systemPrompt,
            ["input"] = input,
            ["temperature"] = _temperature
        };

        if (sendReasoning)
            request["reasoning"] = _reasoning;

        return _client.PostAsJsonAsync(_url, request, ct);
    }

    private static string CleanJsonOutput(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstLineEnd = trimmed.IndexOf('\n');
            if (firstLineEnd != -1)
                trimmed = trimmed[(firstLineEnd + 1)..];

            if (trimmed.EndsWith("```"))
                trimmed = trimmed[..^3];
        }
        return trimmed.Trim();
    }

    private static string? ExtractContentFromAiOutput(JsonDocument doc)
    {
        var root = doc.RootElement;

        if (!root.TryGetProperty("output", out var outputArray) || outputArray.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in outputArray.EnumerateArray())
        {
            if (item.TryGetProperty("type", out var typeProp))
            {
                var typeValue = typeProp.GetString();

                if (typeValue == "message" || typeValue == "content")
                {
                    if (item.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.String)
                    {
                        return contentProp.GetString();
                    }

                    if (item.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                    {
                        return textProp.GetString();
                    }
                }
            }
        }

        return null;
    }
}
