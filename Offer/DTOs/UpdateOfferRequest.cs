using System.ComponentModel.DataAnnotations;
using Offer.Models;
using Offer.Models.Enums;

namespace Offer.DTOs;

public record UpdateOfferRequest
{
    [Required]
    [StringLength(OfferLimits.TitleMaxLength, MinimumLength = 3)]
    public required string Title { get; init; }
    [Required]
    [MaxLength(OfferLimits.DescriptionMaxLength)]
    public required string Description { get; init; }
    public decimal Salary { get; init; }
    [Required]
    public Category Category { get; init; }
    public List<string> Stack { get; init; } = new();
    [Required]
    public Experience Experience { get; init; }
    public List<Requirement> Requirements { get; init; } = new();
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    [MaxLength(OfferLimits.CompanyMaxLength)]
    public string? Company { get; init; }
}
