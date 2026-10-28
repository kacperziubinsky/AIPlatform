namespace User.DTOs;

public record CvDetailsResponse
{
    public required string ParseStatus { get; init; }
    public string? ParseError { get; init; }
    public CvResponse? Data { get; init; }
}
