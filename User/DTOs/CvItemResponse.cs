namespace User.DTOs;

public record CvItemResponse
{
    public Guid Id { get; init; }
    public string FileName { get; init; }
    public DateTime UploadedAt { get; init; }
    public string ParseStatus { get; init; } = "";
}