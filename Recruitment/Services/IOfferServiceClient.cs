using Recruitment.DTOs;

namespace Recruitment.Services;

public interface IOfferServiceClient
{
    Task<OfferDto?> GetOfferAsync(Guid offerId, CancellationToken ct = default);
}
