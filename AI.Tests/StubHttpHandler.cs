using System.Net;

namespace AI.Tests;

public class StubHttpHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();

    public List<string> RequestBodies { get; } = new();

    public StubHttpHandler Returns(HttpResponseMessage response)
    {
        _responses.Enqueue(response);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        RequestBodies.Add(request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct));

        return _responses.Count > 0
            ? _responses.Dequeue()
            : new HttpResponseMessage(HttpStatusCode.InternalServerError);
    }
}
