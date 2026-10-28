using System.Text.Json;
using Recruitment.Models.Enums;

namespace Recruitment.DTOs;

public record OfferDto
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public OfferExperience Experience { get; init; }
    public List<string> Stack { get; init; } = new();
    public List<OfferRequirementDto> Requirements { get; init; } = new();
    public DateTime EndDate { get; init; }
}

public record OfferRequirementDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public int Weight { get; init; }
    public bool IsRequired { get; init; }
}

public record CvDto
{
    public string ParseStatus { get; init; } = "";
    public string? ParseError { get; init; }

    public JsonElement? Data { get; init; }
}

public record AiEvaluationRequest(AiOfferDto Offer, JsonElement Candidate);

public record AiOfferDto(string Title, string Description, string Experience, List<AiRequirementDto> Requirements);

public record AiRequirementDto(Guid Id, string Name, string? Description);

public record AiEvaluationResult
{
    public List<AiAssessmentDto> Assessments { get; init; } = new();
    public string? Summary { get; init; }
}

public record AiAssessmentDto(Guid RequirementId, string Level, string? Evidence);
