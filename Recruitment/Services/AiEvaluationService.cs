using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models;
using Recruitment.Models.Enums;

namespace Recruitment.Services;

public class AiEvaluationService : IAiEvaluationService
{
    private const int MaxAttempts = 5;
    private const int MaxSummaryLength = 2000;
    private const int MaxErrorLength = 1000;

    private static readonly TimeSpan CvNotReadyDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RetryBaseDelay = TimeSpan.FromSeconds(30);

    private readonly RecruitmentDbContext _context;
    private readonly IOfferServiceClient _offers;
    private readonly IUserServiceClient _users;
    private readonly IAiServiceClient _ai;
    private readonly ILogger<AiEvaluationService> _logger;

    public AiEvaluationService(
        RecruitmentDbContext context,
        IOfferServiceClient offers,
        IUserServiceClient users,
        IAiServiceClient ai,
        ILogger<AiEvaluationService> logger)
    {
        _context = context;
        _offers = offers;
        _users = users;
        _ai = ai;
        _logger = logger;
    }

    public async Task EvaluateAsync(JobApplication application, CancellationToken ct = default)
    {
        try
        {
            await RunAsync(application, ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // Services down, LLM timeout or malformed LLM output: all worth retrying with backoff.
            application.AiAttempts++;
            _logger.LogWarning(ex, "AI evaluation of application {ApplicationId} failed (attempt {Attempt}/{MaxAttempts})",
                application.Id, application.AiAttempts, MaxAttempts);

            if (application.AiAttempts >= MaxAttempts)
                Fail(application, $"Evaluation failed after {MaxAttempts} attempts: {ex.Message}");
            else
                Postpone(application, RetryBaseDelay * Math.Pow(2, application.AiAttempts - 1));
        }

        application.UpdatedAt = DateTime.UtcNow;
    }

    private async Task RunAsync(JobApplication application, CancellationToken ct)
    {
        var offer = await _offers.GetOfferAsync(application.OfferId, ct);
        if (offer is null)
        {
            Fail(application, "Offer no longer exists.");
            return;
        }

        application.OfferTitle = offer.Title;

        var cv = await _users.GetCvAsync(application.ApplicantId, application.CvId, ct);
        if (cv is null)
        {
            Fail(application, "CV not found.");
            return;
        }

        if (cv.ParseStatus is "Pending" or "Processing")
        {
            Postpone(application, CvNotReadyDelay);
            return;
        }

        if (cv.ParseStatus != "Completed" || cv.Data is null)
        {
            Fail(application, $"CV could not be parsed: {cv.ParseError}");
            return;
        }

        var requirements = GetRequirements(offer);
        if (requirements.Count == 0)
        {
            Fail(application, "Offer has no requirements or stack to evaluate against.");
            return;
        }

        var result = await _ai.EvaluateAsync(new AiEvaluationRequest(
            new AiOfferDto(
                offer.Title,
                offer.Description,
                offer.Experience.ToString(),
                requirements.Select(r => new AiRequirementDto(r.Id, r.Name, r.Description)).ToList()),
            cv.Data.Value), ct);

        var answers = result.Assessments.ToDictionary(a => a.RequirementId);

        var breakdown = requirements.Select(r =>
        {
            var answer = answers.GetValueOrDefault(r.Id)
                         ?? throw new InvalidOperationException($"AI response is missing requirement '{r.Name}'.");

            if (!Enum.TryParse<MatchLevel>(answer.Level, ignoreCase: true, out var level))
                throw new InvalidOperationException($"AI response has unknown level '{answer.Level}'.");

            return new RequirementAssessment
            {
                RequirementId = r.Id,
                Name = r.Name,
                Weight = r.Weight,
                IsRequired = r.IsRequired,
                Level = level,
                Evidence = answer.Evidence
            };
        }).ToList();

        var score = ScoreCalculator.Calculate(breakdown);

        _context.Evaluations.Add(new Evaluation
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            Score = score,
            Comment = Truncate(result.Summary, MaxSummaryLength),
            Source = EvaluationSource.AI,
            Breakdown = breakdown,
            CreatedAt = DateTime.UtcNow
        });

        application.AiScore = score;
        application.AiStatus = AiEvaluationStatus.Completed;
        application.AiError = null;
        application.AiNextAttemptAt = null;
    }

    // Offers without explicit requirements are scored against their tech stack, each item weighted equally.
    private static List<OfferRequirementDto> GetRequirements(OfferDto offer)
    {
        if (offer.Requirements.Count > 0)
            return offer.Requirements;

        return offer.Stack
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(s => new OfferRequirementDto { Id = Guid.NewGuid(), Name = s.Trim(), Weight = 1 })
            .ToList();
    }

    private static void Postpone(JobApplication application, TimeSpan delay)
    {
        application.AiStatus = AiEvaluationStatus.Pending;
        application.AiNextAttemptAt = DateTime.UtcNow + delay;
    }

    private static void Fail(JobApplication application, string error)
    {
        application.AiStatus = AiEvaluationStatus.Failed;
        application.AiError = Truncate(error, MaxErrorLength);
        application.AiNextAttemptAt = null;
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
