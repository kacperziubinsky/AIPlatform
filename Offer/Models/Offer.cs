namespace Offer.Models;

public class Offer
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    public Category Category { get; set; }
    public List<String> Stack { get; set; } = new List<string>();
    public Experience Experience { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AuthorID {get; set;}
}