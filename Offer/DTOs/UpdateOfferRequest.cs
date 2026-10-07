using Offer.Models.Enums;

namespace Offer.DTOs;

public record UpdateOfferRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public decimal Salary { get; init; }
    public Category Category { get; init; }
    public List<string> Stack { get; init; } = new();
    public Experience Experience { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? Company { get; init; }
} 