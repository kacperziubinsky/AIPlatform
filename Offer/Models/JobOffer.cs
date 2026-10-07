using Offer.Models.Enums;

namespace Offer.Models;

public class JobOffer
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    public Category Category { get; set; }
    public List<String> Stack { get; set; } = new List<string>();
    public Experience Experience { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid AuthorId {get; set;}
    public string? Company { get; set; }
}