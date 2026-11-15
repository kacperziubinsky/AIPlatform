using Microsoft.EntityFrameworkCore;
using Moq;
using Recruitment.Data;
using Recruitment.DTOs;
using Recruitment.Models.Enums;
using Recruitment.Services;

namespace Recruitment.Tests;

public class ApplicationServiceTests
{
    private readonly Mock<IOfferServiceClient> _offers = new();
    private readonly Mock<IUserServiceClient> _users = new();

    private static RecruitmentDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<RecruitmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private ApplicationService CreateService(RecruitmentDbContext db) => new(db, _offers.Object, _users.Object);

    private void OfferReturns(OfferDto? offer) =>
        _offers.Setup(o => o.GetOfferAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(offer);

    private void CvReturns(CvDto? cv) =>
        _users.Setup(u => u.GetCvAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(cv);


    [Fact]
    public async Task Create_Valid_SavesSubmittedApplication()
    {
        await using var db = CreateDb();
        var applicantId = Guid.NewGuid();
        var offer = TestData.Offer();
        OfferReturns(offer);
        CvReturns(TestData.Cv());

        var result = await CreateService(db).CreateAsync(applicantId, TestData.CreateRequest(offer.Id));

        Assert.NotNull(result);
        Assert.Equal(ApplicationStatus.Submitted, result!.Status);
        var saved = await db.Applications.SingleAsync();
        Assert.Equal(applicantId, saved.ApplicantId);
        Assert.Equal(offer.AuthorId, saved.RecruiterId);
        Assert.Equal("Backend dev", saved.OfferTitle);
        Assert.Equal("ACME", saved.OfferCompany);
    }

    [Fact]
    public async Task Create_AlreadyApplied_ReturnsNull()
    {
        await using var db = CreateDb();
        var existing = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db)
            .CreateAsync(existing.ApplicantId, TestData.CreateRequest(existing.OfferId));

        Assert.Null(result);
        Assert.Equal(1, await db.Applications.CountAsync());
    }

    [Fact]
    public async Task Create_OfferMissing_Throws()
    {
        await using var db = CreateDb();
        OfferReturns(null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService(db).CreateAsync(Guid.NewGuid(), TestData.CreateRequest()));
    }

    [Fact]
    public async Task Create_OfferExpired_Throws()
    {
        await using var db = CreateDb();
        OfferReturns(TestData.Offer(endDate: DateTime.UtcNow.AddDays(-1)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(db).CreateAsync(Guid.NewGuid(), TestData.CreateRequest()));
    }

    [Fact]
    public async Task Create_CvMissing_ThrowsAndSavesNothing()
    {
        await using var db = CreateDb();
        OfferReturns(TestData.Offer());
        CvReturns(null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService(db).CreateAsync(Guid.NewGuid(), TestData.CreateRequest()));

        Assert.Equal(0, await db.Applications.CountAsync());
    }


    [Fact]
    public async Task GetMyApplications_ReturnsOnlyOwn_NewestFirst()
    {
        await using var db = CreateDb();
        var applicantId = Guid.NewGuid();
        var older = await TestData.SeedApplicationAsync(db, applicantId, createdAt: DateTime.UtcNow.AddDays(-2));
        var newer = await TestData.SeedApplicationAsync(db, applicantId, createdAt: DateTime.UtcNow);
        await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db).GetMyApplicationsAsync(applicantId);

        Assert.Equal([newer.Id, older.Id], result.Select(a => a.Id));
        Assert.All(result, a => Assert.Equal("ACME", a.Company));
    }


    [Fact]
    public async Task Withdraw_Valid_SetsWithdrawn()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db).WithdrawAsync(application.ApplicantId, application.Id);

        Assert.True(result);
        Assert.Equal(ApplicationStatus.Withdrawn, (await db.Applications.FindAsync(application.Id))!.Status);
    }

    [Fact]
    public async Task Withdraw_OtherApplicant_ReturnsFalse()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db).WithdrawAsync(Guid.NewGuid(), application.Id);

        Assert.False(result);
    }

    [Theory]
    [InlineData(ApplicationStatus.Hired)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Withdrawn)]
    public async Task Withdraw_FinalStatus_Throws(ApplicationStatus status)
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db, status: status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(db).WithdrawAsync(application.ApplicantId, application.Id));
    }


    [Fact]
    public async Task GetForOffer_ReturnsOnlyRecruitersApplications_ScoredFirst()
    {
        await using var db = CreateDb();
        var recruiterId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var unscored = await TestData.SeedApplicationAsync(db, recruiterId: recruiterId, offerId: offerId);
        var low = await TestData.SeedApplicationAsync(db, recruiterId: recruiterId, offerId: offerId, aiScore: 40);
        var high = await TestData.SeedApplicationAsync(db, recruiterId: recruiterId, offerId: offerId, aiScore: 90);
        await TestData.SeedApplicationAsync(db, offerId: offerId, aiScore: 100);

        var result = await CreateService(db).GetForOfferAsync(recruiterId, offerId);

        Assert.Equal([high.Id, low.Id, unscored.Id], result.Select(a => a.Id));
    }

    [Fact]
    public async Task GetById_Owned_ReturnsApplicationWithEvaluations()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db, coverLetter: "List");
        var service = CreateService(db);
        await service.AddEvaluationAsync(application.RecruiterId!.Value, application.Id, TestData.EvaluationRequest(70));

        var result = await service.GetByIdAsync(application.RecruiterId!.Value, application.Id);

        Assert.Equal("List", result!.CoverLetter);
        Assert.Equal(70, Assert.Single(result.Evaluations).Score);
    }

    [Fact]
    public async Task GetById_OtherRecruiter_ReturnsNull()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db).GetByIdAsync(Guid.NewGuid(), application.Id);

        Assert.Null(result);
    }


    [Fact]
    public async Task ChangeStatus_Forward_UpdatesStatusAndNote()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db).ChangeStatusAsync(
            application.RecruiterId!.Value, application.Id, TestData.StatusRequest(ApplicationStatus.Interview));

        Assert.True(result);
        var saved = await db.Applications.FindAsync(application.Id);
        Assert.Equal(ApplicationStatus.Interview, saved!.Status);
        Assert.Equal("notatka", saved.RecruiterNote);
    }

    [Fact]
    public async Task ChangeStatus_Backwards_Throws()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db, status: ApplicationStatus.Interview);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(db).ChangeStatusAsync(
                application.RecruiterId!.Value, application.Id, TestData.StatusRequest(ApplicationStatus.InReview)));
    }

    [Fact]
    public async Task ChangeStatus_OtherRecruiter_ReturnsFalse()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db).ChangeStatusAsync(Guid.NewGuid(), application.Id, TestData.StatusRequest());

        Assert.False(result);
    }


    [Fact]
    public async Task AddEvaluation_Owned_SavesRecruiterEvaluation()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db)
            .AddEvaluationAsync(application.RecruiterId!.Value, application.Id, TestData.EvaluationRequest(85));

        Assert.Equal(85, result!.Score);
        Assert.Equal(EvaluationSource.Recruiter, result.Source);
        Assert.Equal(application.RecruiterId, (await db.Evaluations.SingleAsync()).EvaluatedBy);
    }

    [Fact]
    public async Task AddEvaluation_OtherRecruiter_ReturnsNull()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db)
            .AddEvaluationAsync(Guid.NewGuid(), application.Id, TestData.EvaluationRequest());

        Assert.Null(result);
        Assert.Equal(0, await db.Evaluations.CountAsync());
    }


    [Fact]
    public async Task RequestAiEvaluation_Owned_ResetsAiState()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db, aiStatus: AiEvaluationStatus.Failed, aiAttempts: 5);

        var result = await CreateService(db).RequestAiEvaluationAsync(application.RecruiterId!.Value, application.Id);

        Assert.True(result);
        var saved = await db.Applications.FindAsync(application.Id);
        Assert.Equal(AiEvaluationStatus.Pending, saved!.AiStatus);
        Assert.Equal(0, saved.AiAttempts);
    }

    [Fact]
    public async Task RequestAiEvaluation_OtherRecruiter_ReturnsFalse()
    {
        await using var db = CreateDb();
        var application = await TestData.SeedApplicationAsync(db);

        var result = await CreateService(db).RequestAiEvaluationAsync(Guid.NewGuid(), application.Id);

        Assert.False(result);
    }
}
