using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.Data;
using User.DTOs;
using User.Models;
using User.Models.Enums;

namespace User.Tests;

public static class TestData
{
    public static readonly byte[] PdfBytes = "%PDF-1.7 fake content"u8.ToArray();
    public static readonly byte[] DocxBytes = [(byte)'P', (byte)'K', 3, 4, 0, 0];

    public static ControllerContext ControllerContext(Guid? userId = null, string? apiKey = null)
    {
        var claims = new List<Claim>();

        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        };

        if (apiKey != null)
            http.Request.Headers["X-Api-Key"] = apiKey;

        return new ControllerContext { HttpContext = http };
    }

    public static IFormFile File(string fileName = "cv.pdf", byte[]? content = null, string contentType = "application/pdf")
    {
        var bytes = content ?? PdfBytes;
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    public static UpdateUserRequest UpdateRequest(
        string? firstname = "Jan",
        string? lastname = "Kowalski",
        string? phone = "123456789",
        string? city = "Warszawa",
        List<string>? stack = null,
        Experience experience = Experience.Junior) => new()
    {
        Firstname = firstname,
        Lastname = lastname,
        Phone = phone,
        City = city,
        Stack = stack ?? ["C#"],
        Experience = experience
    };

    public static UserResponse UserResponse(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Firstname = "Jan",
        Lastname = "Kowalski"
    };

    public static CvDetailsResponse CvDetails(string status = "Completed") => new()
    {
        ParseStatus = status,
        Data = new CvResponse { FullName = "Jan Kowalski" }
    };

    public static async Task<UserProfile> SeedUserAsync(
        UserDbContext db,
        Guid? id = null,
        string? firstname = "Jan")
    {
        var user = new UserProfile
        {
            Id = id ?? Guid.NewGuid(),
            Firstname = firstname,
            Lastname = "Kowalski",
            Stack = ["C#"],
            Experience = Experience.Junior
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<CV> SeedCvAsync(
        UserDbContext db,
        Guid userId,
        string fileName = "cv.pdf",
        CvParseStatus status = CvParseStatus.Completed,
        DateTime? uploadedAt = null,
        DateTime? deletedAt = null,
        ParsedCvData? parsedData = null)
    {
        var cv = new CV
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = fileName,
            StorageKey = $"{Guid.NewGuid()}.pdf",
            ContentType = "application/pdf",
            UploadedAt = uploadedAt ?? DateTime.UtcNow,
            DeletedAt = deletedAt,
            ParseStatus = status,
            ParsedData = parsedData
        };

        db.CVs.Add(cv);
        await db.SaveChangesAsync();
        return cv;
    }
}
