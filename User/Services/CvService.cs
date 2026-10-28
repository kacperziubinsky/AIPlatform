using Microsoft.EntityFrameworkCore;
using User.Data;
using User.DTOs;
using User.Models;
using User.Models.Enums;

namespace User.Services;

public class CvService : ICvService
{
    private readonly UserDbContext _context;
    private readonly IFileStorage _storage;

    
    public CvService(UserDbContext context, IFileStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<bool> UploadCvAsync(IFormFile? file, Guid userId)
    {
        if (file == null || file.Length == 0 || file.Length > 5 * 1024 * 1024)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf" && extension != ".docx")
        {
            return false;
        }

        string storageKey;

        await using (var stream = file.OpenReadStream())
            storageKey = await _storage.SaveAsync(stream, extension);

        var cv = new CV
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = Path.GetFileName(file.FileName),
            StorageKey = storageKey,
            ContentType = file.ContentType,
            UploadedAt = DateTime.UtcNow,
            ParseStatus = CvParseStatus.Pending
        };
        
        _context.CVs.Add(cv);
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IReadOnlyList<CvItemResponse>> GetCvsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is invalid", nameof(userId));
        }

        var cvs = await _context.CVs
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UploadedAt)
            .Select(c => new CvItemResponse
            {
                Id = c.Id,
                FileName = c.FileName,
                UploadedAt = c.UploadedAt,
                ParseStatus = c.ParseStatus.ToString()
            })
            .ToListAsync();

        return cvs;
    }

    public async Task<CvDetailsResponse?> GetCvAsync(Guid userId, Guid cvId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is invalid", nameof(userId));
        }
        
        var cv = await _context.CVs.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cvId);

        if (cv is null)
        {
            return null;
        }

        return new CvDetailsResponse
        {
            ParseStatus = cv.ParseStatus.ToString(),
            ParseError = cv.ParseError,
            Data = cv.ParseStatus == CvParseStatus.Completed && cv.ParsedData is not null
                ? ToCvResponse(cv.ParsedData)
                : null
        };
    }

    private static CvResponse ToCvResponse(ParsedCvData data)
    {
        return new CvResponse()
        {
            FullName = data.FullName,
            Email = data.Email,
            Phone = data.Phone,
            Location = data.Location,
            Skills = data.Skills,
            WorkHistory = data.WorkHistory,
            Education = data.Education,
            Languages = data.Languages,
        };
    }

    public async Task<bool> DeleteCvAsync(Guid userId, Guid cvId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is invalid", nameof(userId));
        }

        if (cvId == Guid.Empty)
        {
            throw new ArgumentException("Cv id is invalid", nameof(userId));
        }
        
        var cv = await _context.CVs.FirstOrDefaultAsync(c => c.Id == cvId && c.UserId == userId);

        if (cv is null)
        {
            return false;
        }

        _context.CVs.Remove(cv);
        await _context.SaveChangesAsync();

        await _storage.DeleteAsync(cv.StorageKey);
        
        return true;
    }
}