using System.ComponentModel.DataAnnotations;
using Offer.Models;
using Offer.Models.Enums;
namespace Offer.DTOs;

public record CreateOfferRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public required string Title { get; set; }
    [Required]
    [MaxLength(5000)]
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    [Required]
    public Category Category { get; set; }
    public List<String> Stack { get; set; } = new List<string>();
    [Required]
    public Experience Experience { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Company { get; set; }
}