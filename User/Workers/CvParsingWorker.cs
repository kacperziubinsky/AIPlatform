using Microsoft.EntityFrameworkCore;
using User.Data;
using User.Models.Enums;
using User.Services;

namespace User.Workers;

/// <summary>
/// Sends uploaded CVs to the AI service one at a time and stores the structured result.
/// </summary>
public class CvParsingWorker(IServiceScopeFactory scopeFactory, ILogger<CvParsingWorker> logger) : BackgroundService
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
                logger.LogError(ex, "CV parsing loop failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    // CVs left in Processing were interrupted by a shutdown. Assumes a single User service instance.
    private async Task RequeueInterruptedAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            await db.CVs
                .Where(c => c.ParseStatus == CvParseStatus.Processing)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ParseStatus, CvParseStatus.Pending), ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Failed to requeue interrupted CVs");
        }
    }

    private async Task<bool> ProcessNextAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var parser = scope.ServiceProvider.GetRequiredService<ICvParserClient>();

        var cvId = await db.CVs
            .Where(c => c.ParseStatus == CvParseStatus.Pending)
            .OrderBy(c => c.UploadedAt)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);

        if (cvId == Guid.Empty)
            return false;

        var claimed = await db.CVs
            .Where(c => c.Id == cvId && c.ParseStatus == CvParseStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ParseStatus, CvParseStatus.Processing), ct);

        if (claimed == 0)
            return true;

        var cv = await db.CVs.FirstAsync(c => c.Id == cvId, ct);

        try
        {
            await using var file = await storage.OpenReadAsync(cv.StorageKey)
                                   ?? throw new FileNotFoundException("Stored CV file is missing.");

            var (data, rawJson) = await parser.ParseAsync(file, cv.FileName, ct);

            cv.ParsedData = data;
            cv.RawAiJson = rawJson;
            cv.ParseStatus = CvParseStatus.Completed;
            cv.ParseError = null;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Parsing CV {CvId} failed", cv.Id);

            cv.ParseStatus = CvParseStatus.Failed;
            cv.ParseError = ex.Message;
        }

        cv.ParsedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
