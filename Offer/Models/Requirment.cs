namespace Offer.Models;

public class Requirement
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Weight { get; set; }
    public bool IsRequired { get; set; }
}