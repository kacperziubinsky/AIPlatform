using Offer.DTOs;

namespace Offer.Services;

public interface IOfferService
{
    Task<OfferResponse> CreateOfferAsync(CreateOfferRequest request, Guid userId);

    Task<OfferResponse?> GetOfferByIdAsync(Guid id);

    Task<List<OfferResponse>> GetOffersAsync(OfferFilterRequest? filter = null);
    
    Task<OfferResponse?> UpdateOfferAsync(Guid id, UpdateOfferRequest request, Guid userId);

    Task<bool> DeleteOfferAsync(Guid id, Guid userId);
}