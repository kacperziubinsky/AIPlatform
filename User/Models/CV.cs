using User.Models.Enums;

namespace User.Models;

public class CV
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string FileName { get; set; } = "";
    public string StorageKey { get; set; } = "";
    public string ContentType { get; set; } = "";
    public DateTime UploadedAt { get; set; }

    public CvParseStatus ParseStatus { get; set; } = CvParseStatus.Pending;
    public DateTime? ParsedAt { get; set; }
    public string? ParseError { get; set; }

    public ParsedCvData? ParsedData { get; set; }
    public string? RawAiJson { get; set; }
}