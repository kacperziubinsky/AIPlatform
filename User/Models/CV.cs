namespace User.Models;

public class CV
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FileName { get; set; }
    public string StorageKey { get; set; }

    public DateTime UploadedAt { get; set; }

    public string ContentType { get; set; }

}