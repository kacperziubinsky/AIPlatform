using System.ComponentModel.DataAnnotations;

namespace Offer.Models.Enums;

public enum Experience
{
    [Display(Name = "Trainee")] Trainee,
    [Display(Name = "Junior")] Junior,
    [Display(Name = "Mid")] Middle,
    [Display(Name = "Senior")] Senior,
}
