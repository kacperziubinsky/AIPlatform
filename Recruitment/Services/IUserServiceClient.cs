using Recruitment.DTOs;

namespace Recruitment.Services;

public interface IUserServiceClient
{
    Task<CvDto?> GetCvAsync(Guid userId, Guid cvId, bool includeDeleted = false, CancellationToken ct = default);
}
