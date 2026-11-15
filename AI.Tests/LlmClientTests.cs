using System.Net;
using Applicant.Models;

namespace AI.Tests;

public class LlmClientTests
{
    private readonly StubHttpHandler _http = new();


    [Fact]
    public async Task CompleteJson_ValidReply_ReturnsObject()
    {
        _http.Returns(TestData.LlmReply(new { full_name = "Jan Kowalski", skills = new[] { "C#" } }));

        var result = await TestData.LlmClient(_http).CompleteJsonAsync<ApplicantProfile>("system", "input");

        Assert.Equal("Jan Kowalski", result!.FullName);
        Assert.Equal(["C#"], result.Skills);
    }

    [Fact]
    public async Task CompleteJson_ReplyInCodeFence_IsUnwrapped()
    {
        _http.Returns(TestData.LlmReply("```json\n{\"full_name\": \"Jan\"}\n```"));

        var result = await TestData.LlmClient(_http).CompleteJsonAsync<ApplicantProfile>("system", "input");

        Assert.Equal("Jan", result!.FullName);
    }

    [Fact]
    public async Task CompleteJson_SendsPromptAndInput()
    {
        _http.Returns(TestData.LlmReply("{}"));

        await TestData.LlmClient(_http).CompleteJsonAsync<ApplicantProfile>("my system prompt", "my input");

        var body = Assert.Single(_http.RequestBodies);
        Assert.Contains("\"system_prompt\":\"my system prompt\"", body);
        Assert.Contains("\"input\":\"my input\"", body);
    }

    [Fact]
    public async Task CompleteJson_InvalidJson_ReturnsNull()
    {
        _http.Returns(TestData.LlmReply("to nie jest json"));

        var result = await TestData.LlmClient(_http).CompleteJsonAsync<ApplicantProfile>("system", "input");

        Assert.Null(result);
    }

    [Fact]
    public async Task CompleteJson_NoMessageInOutput_ReturnsNull()
    {
        _http.Returns(TestData.LlmRaw("{\"output\": [{\"type\": \"reasoning\", \"content\": \"...\"}]}"));

        var result = await TestData.LlmClient(_http).CompleteJsonAsync<ApplicantProfile>("system", "input");

        Assert.Null(result);
    }

    [Fact]
    public async Task CompleteJson_ServerError_Throws()
    {
        _http.Returns(TestData.LlmRaw("boom", HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            TestData.LlmClient(_http).CompleteJsonAsync<ApplicantProfile>("system", "input"));
    }

    [Fact]
    public async Task CompleteJson_ModelRejectsReasoning_RetriesWithoutIt()
    {
        _http.Returns(TestData.LlmRaw("{\"error\": {\"param\": \"reasoning\"}}", HttpStatusCode.BadRequest))
             .Returns(TestData.LlmReply(new { full_name = "Jan" }));
        var client = TestData.LlmClient(_http);

        var result = await client.CompleteJsonAsync<ApplicantProfile>("system", "input");

        Assert.Equal("Jan", result!.FullName);
        Assert.Contains("\"reasoning\"", _http.RequestBodies[0]);
        Assert.DoesNotContain("\"reasoning\"", _http.RequestBodies[1]);
    }

    [Fact]
    public async Task CompleteJson_ModelRejectedReasoningBefore_SkipsItNextTime()
    {
        _http.Returns(TestData.LlmRaw("\"reasoning\" not supported", HttpStatusCode.BadRequest))
             .Returns(TestData.LlmReply("{}"))
             .Returns(TestData.LlmReply("{}"));
        var client = TestData.LlmClient(_http);

        await client.CompleteJsonAsync<ApplicantProfile>("system", "input");
        await client.CompleteJsonAsync<ApplicantProfile>("system", "input");

        Assert.Equal(3, _http.RequestBodies.Count);
        Assert.DoesNotContain("\"reasoning\"", _http.RequestBodies[2]);
    }
}
