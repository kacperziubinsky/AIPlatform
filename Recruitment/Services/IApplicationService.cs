using Recruitment.DTOs;

namespace Recruitment.Services;

public interface IApplicationService
{
    Task<ApplicationResponse?> CreateAsync(Guid applicantId, CreateApplicationRequest request);
    Task<List<ApplicationResponse>> GetMyApplicationsAsync(Guid applicantId);
    Task<bool> WithdrawAsync(Guid applicantId, Guid applicationId);

    Task<List<RecruiterApplicationResponse>> GetForOfferAsync(Guid recruiterId, Guid offerId);
    Task<RecruiterApplicationResponse?> GetByIdAsync(Guid recruiterId, Guid applicationId);
    Task<bool> ChangeStatusAsync(Guid recruiterId, Guid applicationId, ChangeStatusRequest request);
    Task<bool> RequestAiEvaluationAsync(Guid recruiterId, Guid applicationId);
    Task<EvaluationResponse?> AddEvaluationAsync(Guid recruiterId, Guid applicationId, CreateEvaluationRequest request);
}