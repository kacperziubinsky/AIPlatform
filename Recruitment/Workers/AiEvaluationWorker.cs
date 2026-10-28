using Microsoft.EntityFrameworkCore;
using Recruitment.Data;
using Recruitment.Models.Enums;
using Recruitment.Services;

namespace Recruitment.Workers;


public class AiEvaluationWorker(IServiceScopeFactory scopeFactory, ILogger<AiEvaluationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RequeueInterruptedAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                while (await ProcessNextAsync(stoppingToken))
                {
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "AI evaluation loop failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task RequeueInterruptedAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();

            await db.Applications
                .Where(a => a.AiStatus == AiEvaluationStatus.Processing)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.AiStatus, AiEvaluationStatus.Pending), ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Failed to requeue interrupted AI evaluations");
        }
    }

    private async Task<bool> ProcessNextAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();
        var evaluator = scope.ServiceProvider.GetRequiredService<IAiEvaluationService>();

        var now = DateTime.UtcNow;
        var applicationId = await db.Applications
            .Where(a => a.AiStatus == AiEvaluationStatus.Pending
                        && a.Status != ApplicationStatus.Withdrawn
                        && (a.AiNextAttemptAt == null || a.AiNextAttemptAt <= now))
            .OrderBy(a => a.CreatedAt)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(ct);

        if (applicationId == Guid.Empty)
            return false;

        var claimed = await db.Applications
            .Where(a => a.Id == applicationId && a.AiStatus == AiEvaluationStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.AiStatus, AiEvaluationStatus.Processing), ct);

        if (claimed == 0)
            return true;

        var application = await db.Applications.FirstAsync(a => a.Id == applicationId, ct);

        await evaluator.EvaluateAsync(application, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
