using Microsoft.EntityFrameworkCore;
using Offer.Data;
using Offer.DTOs;
using Offer.Models;
using Offer.Services;

namespace Offer.Tests;

public class OfferServiceTests
{
    private static OfferDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<OfferDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);


    [Fact]
    public async Task CreateOffer_Valid_SavesOfferWithAuthor()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();

        var result = await new OfferService(db).CreateOfferAsync(TestData.CreateRequest(), userId);

        Assert.Equal(userId, result.AuthorId);
        Assert.Equal("Backend dev", result.Title);
        Assert.Equal(1, await db.Offers.CountAsync());
    }

    [Fact]
    public async Task CreateOffer_NullStackAndRequirements_BecomeEmptyLists()
    {
        await using var db = CreateDb();

        var result = await new OfferService(db)
            .CreateOfferAsync(TestData.CreateRequest(stack: null, requirements: null), Guid.NewGuid());

        Assert.Empty(result.Stack);
        Assert.Empty(result.Requirements);
    }

    [Fact]
    public async Task CreateOffer_Requirements_AreTrimmedAndGetNewIds()
    {
        await using var db = CreateDb();
        var requirements = new List<Requirement>
        {
            new() { Name = "  C#  ", Description = "  dobrze  " }
        };

        var result = await new OfferService(db)
            .CreateOfferAsync(TestData.CreateRequest(requirements: requirements), Guid.NewGuid());

        var r = Assert.Single(result.Requirements);
        Assert.Equal("C#", r.Name);
        Assert.Equal("dobrze", r.Description);
        Assert.NotEqual(Guid.Empty, r.Id);
    }

    [Fact]
    public async Task CreateOffer_RequirementWithEmptyName_Throws()
    {
        await using var db = CreateDb();
        var requirements = new List<Requirement> { new() { Name = " " , Description = " " } };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new OfferService(db).CreateOfferAsync(TestData.CreateRequest(requirements: requirements), Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateOffer_EmptyTitle_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(title: " "));

    [Fact]
    public async Task CreateOffer_TooLongTitle_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(title: new string('a', OfferLimits.TitleMaxLength + 1)));

    [Fact]
    public async Task CreateOffer_EmptyDescription_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(description: ""));

    [Fact]
    public async Task CreateOffer_TooLongDescription_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(description: new string('a', OfferLimits.DescriptionMaxLength + 1)));

    [Fact]
    public async Task CreateOffer_TooLongCompany_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(company: new string('a', OfferLimits.CompanyMaxLength + 1)));

    [Fact]
    public async Task CreateOffer_InvalidCategory_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(category: TestData.InvalidCategory));

    [Fact]
    public async Task CreateOffer_InvalidExperience_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(experience: TestData.InvalidExperience));

    [Fact]
    public async Task CreateOffer_StartDateInPast_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(
            start: DateTime.UtcNow.AddDays(-2), end: DateTime.UtcNow.AddDays(5)));

    [Fact]
    public async Task CreateOffer_EndDateEqualsStartDate_Throws()
    {
        var start = DateTime.UtcNow.AddDays(1);
        await AssertCreateThrows(TestData.CreateRequest(start: start, end: start));
    }

    [Fact]
    public async Task CreateOffer_EndDateBeforeStartDate_Throws() =>
        await AssertCreateThrows(TestData.CreateRequest(
            start: DateTime.UtcNow.AddDays(5), end: DateTime.UtcNow.AddDays(2)));

    private static async Task AssertCreateThrows(CreateOfferRequest request)
    {
        await using var db = CreateDb();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new OfferService(db).CreateOfferAsync(request, Guid.NewGuid()));

        Assert.Equal(0, await db.Offers.CountAsync());
    }


    [Fact]
    public async Task GetOfferById_Existing_ReturnsOffer()
    {
        await using var db = CreateDb();
        var seeded = await TestData.SeedOfferAsync(db);

        var result = await new OfferService(db).GetOfferByIdAsync(seeded.Id);

        Assert.NotNull(result);
        Assert.Equal(seeded.Id, result!.Id);
    }

    [Fact]
    public async Task GetOfferById_Missing_ReturnsNull()
    {
        await using var db = CreateDb();

        var result = await new OfferService(db).GetOfferByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }


    [Fact]
    public async Task GetOffersByAuthor_ReturnsOnlyAuthorsOffers_Newest_First ()
    {
        await using var db = CreateDb();
        var author = Guid.NewGuid();
        await TestData.SeedOfferAsync(db, authorId: author, title: "stara", createdOn: DateTime.UtcNow.AddDays(-2));
        await TestData.SeedOfferAsync(db, authorId: author, title: "nowa", createdOn: DateTime.UtcNow);
        await TestData.SeedOfferAsync(db, authorId: Guid.NewGuid(), title: "cudza");

        var result = await new OfferService(db).GetOffersByAuthorAsync(author);

        Assert.Equal(new[] { "nowa", "stara" }, result.Select(o => o.Title));
    }


    [Fact]
    public async Task UpdateOffer_Valid_UpdatesFields()
    {
        await using var db = CreateDb();
        var author = Guid.NewGuid();
        var seeded = await TestData.SeedOfferAsync(db, authorId: author);

        var result = await new OfferService(db).UpdateOfferAsync(
            seeded.Id,
            TestData.UpdateRequest(title: "Nowy tytuł", category: TestData.CategoryB),
            author);

        Assert.NotNull(result);
        Assert.Equal("Nowy tytuł", result!.Title);
        Assert.Equal(TestData.CategoryB, result.Category);
        Assert.Equal("Nowy tytuł", (await db.Offers.FindAsync(seeded.Id))!.Title);
    }

    [Fact]
    public async Task UpdateOffer_NotFound_Throws()
    {
        await using var db = CreateDb();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new OfferService(db).UpdateOfferAsync(Guid.NewGuid(), TestData.UpdateRequest(), Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateOffer_DifferentUser_ThrowsUnauthorized()
    {
        await using var db = CreateDb();
        var seeded = await TestData.SeedOfferAsync(db, authorId: Guid.NewGuid());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new OfferService(db).UpdateOfferAsync(seeded.Id, TestData.UpdateRequest(), Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateOffer_InvalidData_Throws()
    {
        await using var db = CreateDb();
        var author = Guid.NewGuid();
        var seeded = await TestData.SeedOfferAsync(db, authorId: author);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new OfferService(db).UpdateOfferAsync(seeded.Id, TestData.UpdateRequest(title: ""), author));
    }

    [Fact]
    public async Task UpdateOffer_PastStartDateUnchanged_IsAllowed()
    {
        await using var db = CreateDb();
        var author = Guid.NewGuid();
        var pastStart = DateTime.UtcNow.AddDays(-5);
        var seeded = await TestData.SeedOfferAsync(db, authorId: author, start: pastStart, end: DateTime.UtcNow.AddDays(10));

        var result = await new OfferService(db).UpdateOfferAsync(
            seeded.Id,
            TestData.UpdateRequest(title: "Zmiana", start: pastStart, end: DateTime.UtcNow.AddDays(10)),
            author);

        Assert.Equal("Zmiana", result!.Title);
    }

    [Fact]
    public async Task UpdateOffer_StartDateChangedToPast_Throws()
    {
        await using var db = CreateDb();
        var author = Guid.NewGuid();
        var seeded = await TestData.SeedOfferAsync(db, authorId: author);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new OfferService(db).UpdateOfferAsync(
                seeded.Id,
                TestData.UpdateRequest(start: DateTime.UtcNow.AddDays(-3), end: DateTime.UtcNow.AddDays(5)),
                author));
    }


    [Fact]
    public async Task DeleteOffer_Valid_RemovesOffer()
    {
        await using var db = CreateDb();
        var author = Guid.NewGuid();
        var seeded = await TestData.SeedOfferAsync(db, authorId: author);

        var result = await new OfferService(db).DeleteOfferAsync(seeded.Id, author);

        Assert.True(result);
        Assert.Equal(0, await db.Offers.CountAsync());
    }

    [Fact]
    public async Task DeleteOffer_NotFound_Throws()
    {
        await using var db = CreateDb();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new OfferService(db).DeleteOfferAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteOffer_DifferentUser_ThrowsAndKeepsOffer()
    {
        await using var db = CreateDb();
        var seeded = await TestData.SeedOfferAsync(db, authorId: Guid.NewGuid());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new OfferService(db).DeleteOfferAsync(seeded.Id, Guid.NewGuid()));

        Assert.Equal(1, await db.Offers.CountAsync());
    }
    

    [Fact]
    public async Task GetOffers_ExcludesExpiredOffers()
    {
        await using var db = CreateDb();
        await TestData.SeedOfferAsync(db, title: "wygasła",
            start: DateTime.UtcNow.AddDays(-10), end: DateTime.UtcNow.AddDays(-1));
        await TestData.SeedOfferAsync(db, title: "aktywna");

        var result = await new OfferService(db).GetOffersAsync(null);

        Assert.Equal("aktywna", Assert.Single(result).Title);
    }

    [Fact]
    public async Task GetOffers_OrdersByCreatedOnDescending()
    {
        await using var db = CreateDb();
        await TestData.SeedOfferAsync(db, title: "najstarsza", createdOn: DateTime.UtcNow.AddDays(-3));
        await TestData.SeedOfferAsync(db, title: "najnowsza", createdOn: DateTime.UtcNow);
        await TestData.SeedOfferAsync(db, title: "środkowa", createdOn: DateTime.UtcNow.AddDays(-1));

        var result = await new OfferService(db).GetOffersAsync(null);

        Assert.Equal(new[] { "najnowsza", "środkowa", "najstarsza" }, result.Select(o => o.Title));
    }

    [Fact]
    public async Task GetOffers_FilterByCategory()
    {
        await using var db = CreateDb();
        await TestData.SeedOfferAsync(db, title: "A", category: TestData.CategoryA);
        await TestData.SeedOfferAsync(db, title: "B", category: TestData.CategoryB);

        var result = await new OfferService(db)
            .GetOffersAsync(new OfferFilterRequest { Category = TestData.CategoryB });

        Assert.Equal("B", Assert.Single(result).Title);
    }

    [Fact]
    public async Task GetOffers_FilterByExperience()
    {
        await using var db = CreateDb();
        await TestData.SeedOfferAsync(db, title: "A", experience: TestData.ExperienceA);
        await TestData.SeedOfferAsync(db, title: "B", experience: TestData.ExperienceB);

        var result = await new OfferService(db)
            .GetOffersAsync(new OfferFilterRequest { Experience = TestData.ExperienceB });

        Assert.Equal("B", Assert.Single(result).Title);
    }

    [Fact]
    public async Task GetOffers_FilterBySalaryRange()
    {
        await using var db = CreateDb();
        await TestData.SeedOfferAsync(db, title: "niska", salary: 3000);
        await TestData.SeedOfferAsync(db, title: "średnia", salary: 8000);
        await TestData.SeedOfferAsync(db, title: "wysoka", salary: 20000);

        var result = await new OfferService(db)
            .GetOffersAsync(new OfferFilterRequest { MinSalary = 5000, MaxSalary = 10000 });

        Assert.Equal("średnia", Assert.Single(result).Title);
    }

    [Fact]
    public async Task GetOffers_RespectsLimit()
    {
        await using var db = CreateDb();
        for (var i = 0; i < 5; i++)
            await TestData.SeedOfferAsync(db, title: $"o{i}", createdOn: DateTime.UtcNow.AddMinutes(-i));

        var result = await new OfferService(db).GetOffersAsync(new OfferFilterRequest { Limit = 2 });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetOffers_NonPositiveLimit_FallsBackToDefault()
    {
        await using var db = CreateDb();
        for (var i = 0; i < 3; i++)
            await TestData.SeedOfferAsync(db, title: $"o{i}");

        var result = await new OfferService(db).GetOffersAsync(new OfferFilterRequest { Limit = 0 });

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllOffers_RespectsLimit()
    {
        await using var db = CreateDb();
        for (var i = 0; i < 4; i++)
            await TestData.SeedOfferAsync(db, title: $"o{i}");

        var result = await new OfferService(db).GetAllOffersAsync(limit: 3);

        Assert.Equal(3, result.Count);
    }
}