using System.Net;
using Recruitment.DTOs;

namespace Recruitment.Services;

public class OfferServiceClient(HttpClient client) : IOfferServiceClient
{
    public async Task<OfferDto?> GetOfferAsync(Guid offerId, CancellationToken ct = default)
    {
        using var response = await client.GetAsync($"offers/{offerId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfferDto>(ct);
    }
}
