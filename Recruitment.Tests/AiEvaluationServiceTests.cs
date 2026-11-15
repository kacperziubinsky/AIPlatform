using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models.Enums;
using Recruitment.Services;

namespace Recruitment.Tests;

public class AiEvaluationServiceTests
{
    private readonly Mock<IOfferServiceClient> _offers = new();
    private readonly Mock<IUserServiceClient> _users = new();
    private readonly Mock<IAiServiceClient> _ai = new();

    public AiEvaluationServiceTests()
    {
        CvReturns(TestData.Cv());
    }

    private static RecruitmentDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<RecruitmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private AiEvaluationService CreateService(RecruitmentDbContext db) =>
        new(db, _offers.Object, _users.Object, _ai.Object, NullLogger<AiEvaluationService>.Instance);

    private void OfferReturns(OfferDto? offer) =>
        _offers.Setup(o => o.GetOfferAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);

    private void CvReturns(CvDto? cv) =>
        _users.Setup(u => u.GetCvAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(cv);

    private void AiReturns(AiEvaluationResult result) =>
        _ai.Setup(a => a.EvaluateAsync(It.IsAny<AiEvaluationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);


    [Fact]
    public async Task Evaluate_Valid_SavesAiEvaluationAndScore()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        var csharp = TestData.Requirement("C#");
        var sql = TestData.Requirement("SQL");
        OfferReturns(TestData.Offer(requirements: [csharp, sql]));
        AiReturns(TestData.AiResult((csharp.Id, "full"), (sql.Id, "none")));

        await CreateService(db).EvaluateAsync(application);
        await db.SaveChangesAsync();

        Assert.Equal(AiEvaluationStatus.Completed, application.AiStatus);
        Assert.Equal(50, application.AiScore);
        var evaluation = await db.Evaluations.SingleAsync();
        Assert.Equal(EvaluationSource.AI, evaluation.Source);
        Assert.Equal("Pasuje.", evaluation.Comment);
        Assert.Equal([MatchLevel.Full, MatchLevel.None], evaluation.Breakdown.Select(b => b.Level));
    }

    [Fact]
    public async Task Evaluate_NoRequirements_UsesStackInstead()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(TestData.Offer(stack: ["C#", "c#", " ", "Docker"]));
        _ai.Setup(a => a.EvaluateAsync(It.IsAny<AiEvaluationRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync((AiEvaluationRequest r, CancellationToken _) =>
               TestData.AiResult(r.Offer.Requirements.Select(x => (x.Id, "full")).ToArray()));

        await CreateService(db).EvaluateAsync(application);

        _ai.Verify(a => a.EvaluateAsync(
            It.Is<AiEvaluationRequest>(r => r.Offer.Requirements.Select(x => x.Name).SequenceEqual(new[] { "C#", "Docker" })),
            It.IsAny<CancellationToken>()));
        Assert.Equal(100, application.AiScore);
    }

    [Fact]
    public async Task Evaluate_OfferMissing_Fails()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(null);

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Failed, application.AiStatus);
        Assert.Equal("Offer no longer exists.", application.AiError);
    }

    [Fact]
    public async Task Evaluate_CvMissing_Fails()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(TestData.Offer(requirements: [TestData.Requirement()]));
        CvReturns(null);

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Failed, application.AiStatus);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Processing")]
    public async Task Evaluate_CvNotParsedYet_PostponesWithoutCallingAi(string status)
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(TestData.Offer(requirements: [TestData.Requirement()]));
        CvReturns(TestData.Cv(status));

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Pending, application.AiStatus);
        Assert.True(application.AiNextAttemptAt > DateTime.UtcNow);
        Assert.Equal(0, application.AiAttempts);
        _ai.Verify(a => a.EvaluateAsync(It.IsAny<AiEvaluationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Evaluate_CvParseFailed_Fails()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(TestData.Offer(requirements: [TestData.Requirement()]));
        CvReturns(TestData.Cv("Failed", error: "zły plik"));

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Failed, application.AiStatus);
        Assert.Contains("zły plik", application.AiError);
    }

    [Fact]
    public async Task Evaluate_OfferWithoutRequirementsOrStack_Fails()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(TestData.Offer());

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Failed, application.AiStatus);
    }

    [Fact]
    public async Task Evaluate_AiServiceDown_RetriesLater()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(TestData.Offer(requirements: [TestData.Requirement()]));
        _ai.Setup(a => a.EvaluateAsync(It.IsAny<AiEvaluationRequest>(), It.IsAny<CancellationToken>()))
           .ThrowsAsync(new HttpRequestException("down"));

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Pending, application.AiStatus);
        Assert.Equal(1, application.AiAttempts);
        Assert.True(application.AiNextAttemptAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Evaluate_AiAnswerMissingRequirement_RetriesLater()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);
        OfferReturns(TestData.Offer(requirements: [TestData.Requirement()]));
        AiReturns(TestData.AiResult((Guid.NewGuid(), "full")));

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Pending, application.AiStatus);
        Assert.Equal(1, application.AiAttempts);
    }

    [Fact]
    public async Task Evaluate_LastAttemptFails_GivesUp()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db, aiAttempts: 4);
        OfferReturns(TestData.Offer(requirements: [TestData.Requirement()]));
        _ai.Setup(a => a.EvaluateAsync(It.IsAny<AiEvaluationRequest>(), It.IsAny<CancellationToken>()))
           .ThrowsAsync(new HttpRequestException("down"));

        await CreateService(db).EvaluateAsync(application);

        Assert.Equal(AiEvaluationStatus.Failed, application.AiStatus);
        Assert.Equal(5, application.AiAttempts);
        Assert.Null(application.AiNextAttemptAt);
    }
}
