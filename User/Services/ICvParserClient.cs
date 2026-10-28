using User.Models;

namespace User.Services;

public interface ICvParserClient
{
    Task<(ParsedCvData Data, string RawJson)> ParseAsync(Stream content, string fileName, CancellationToken ct = default);
}
