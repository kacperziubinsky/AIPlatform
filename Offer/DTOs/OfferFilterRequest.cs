using Offer.Models.Enums;

namespace Offer.DTOs;

public record OfferFilterRequest(
    string? SearchTerm = null,
    Category? Category = null,
    Experience? Experience = null,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    int Limit = 50
);