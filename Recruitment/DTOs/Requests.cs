using System.ComponentModel.DataAnnotations;
using Recruitment.Models.Enums;

namespace Recruitment.DTOs;

public record CreateApplicationRequest
{
    [Required] public Guid OfferId { get; init; }
    [Required] public Guid CvId { get; init; }
    [MaxLength(3000)] public string? CoverLetter { get; init; }
}

public record ChangeStatusRequest
{
    [Required, EnumDataType(typeof(ApplicationStatus))]
    public ApplicationStatus Status { get; init; }

    [MaxLength(1000)] public string? Note { get; init; }
}

public record CreateEvaluationRequest
{
    [Range(0, 100)] public int Score { get; init; }
    [MaxLength(2000)] public string? Comment { get; init; }
}