using Microsoft.AspNetCore.Http;
using User.DTOs;

namespace User.Services;

public interface ICvService
{
    Task<bool> UploadCvAsync(IFormFile? file, Guid userId);
    Task<IReadOnlyList<CvItemResponse>> GetCvsAsync(Guid userId);
    Task<CvDetailsResponse?> GetCvAsync(Guid userId, Guid cvId);
    Task<bool> DeleteCvAsync(Guid userId, Guid cvId);
}