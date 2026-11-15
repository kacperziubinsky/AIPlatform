using System.Net;
using System.Text.Json;
using Applicant.Models;
using Applicant.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace AI.Tests;

public static class TestData
{
    public static LlmClient LlmClient(StubHttpHandler handler, string reasoning = "off") =>
        new(new HttpClient(handler), new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Llm:Url"] = "http://llm.test/api/v1/chat",
                ["Llm:Model"] = $"model-{Guid.NewGuid()}",
                ["Llm:Reasoning"] = reasoning
            })
            .Build());

    public static HttpResponseMessage LlmReply(string content) =>
        LlmRaw(JsonSerializer.Serialize(new { output = new[] { new { type = "message", content } } }));

    public static HttpResponseMessage LlmReply(object json) =>
        LlmReply(JsonSerializer.Serialize(json));

    public static HttpResponseMessage LlmRaw(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(body) };

    public static EvaluationRequest EvaluationRequest(params string[] requirements) => new()
    {
        Offer = new EvaluationOffer
        {
            Title = "Backend dev",
            Description = "Opis",
            Experience = "Junior",
            Requirements = requirements
                .Select(name => new EvaluationRequirement { Id = Guid.NewGuid(), Name = name })
                .ToList()
        },
        Candidate = new EvaluationCandidate { Skills = ["C#"] }
    };

    public static object Assessment(string id, string level, string? evidence = "Pracował 2 lata w C#") =>
        new { id, level, evidence };

    public static byte[] Pdf(string? text = "Jan Kowalski, C# developer")
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);

        if (text != null)
            page.AddText(text, 12, new PdfPoint(25, 700), builder.AddStandard14Font(Standard14Font.Helvetica));

        return builder.Build();
    }

    public static IFormFile File(byte[] content, string fileName = "cv.pdf") =>
        new FormFile(new MemoryStream(content), 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };
}
