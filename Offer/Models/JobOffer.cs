using Offer.Models;
using Offer.Models.Enums;

public class JobOffer
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    public Category Category { get; set; }
    public List<string> Stack { get; set; } = new();
    public Experience Experience { get; set; }

    public List<Requirement> Requirements { get; set; } = new();

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid AuthorId { get; set; }
    public string? Company { get; set; }
}