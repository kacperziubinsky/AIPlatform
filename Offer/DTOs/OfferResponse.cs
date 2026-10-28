using Offer.Models;
using Offer.Models.Enums;

namespace Offer.DTOs;

public record OfferResponse
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public decimal Salary { get; init; }
    public Category Category { get; init; }
    public List<string> Stack { get; init; } = new();
    public Experience Experience { get; init; }
    public List<Requirement> Requirements { get; init; } = new();
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime CreatedOn { get; init; }
    public string? Company { get; init; }
}