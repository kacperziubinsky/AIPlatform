using System.ComponentModel.DataAnnotations;

namespace Offer.Models.Enums;

public enum Category
{
    [Display(Name = "AI")] Ai,
    [Display(Name = "Backend")] Backend,
    [Display(Name = "Project Management")] Pm,
    [Display(Name = "Full Stack")] FullStack,
    [Display(Name = "Software")] Software,
    [Display(Name = "Frontend")] Frontend
}
