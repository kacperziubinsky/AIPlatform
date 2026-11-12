using CategoryEnum = Offer.Models.Enums.Category;
using ExperienceEnum = Offer.Models.Enums.Experience;

using Offer.Data;
using Offer.DTOs;
using Offer.Models;

namespace Offer.Tests;

public static class TestData
{
    public static readonly CategoryEnum CategoryA = Enum.GetValues<CategoryEnum>()[0];
    public static readonly CategoryEnum CategoryB = Enum.GetValues<CategoryEnum>()[1];
    public static readonly ExperienceEnum ExperienceA = Enum.GetValues<ExperienceEnum>()[0];
    public static readonly ExperienceEnum ExperienceB = Enum.GetValues<ExperienceEnum>()[1];

    public static readonly CategoryEnum InvalidCategory = (CategoryEnum)9999;
    public static readonly ExperienceEnum InvalidExperience = (ExperienceEnum)9999;

    public static CreateOfferRequest CreateRequest(
        string title = "Backend dev",
        string description = "Opis",
        string? company = "ACME",
        CategoryEnum? category = null,
        ExperienceEnum? experience = null,
        DateTime? start = null,
        DateTime? end = null,
        List<Requirement>? requirements = null,
        List<string>? stack = null) => new()
    {
        Title = title,
        Description = description,
        Company = company,
        Salary = 10000,
        Category = category ?? CategoryA,
        Experience = experience ?? ExperienceA,
        StartDate = start ?? DateTime.UtcNow.AddDays(1),
        EndDate = end ?? DateTime.UtcNow.AddDays(30),
        Requirements = requirements,
        Stack = stack
    };

    public static UpdateOfferRequest UpdateRequest(
        string title = "Backend dev",
        string description = "Opis",
        string? company = "ACME",
        CategoryEnum? category = null,
        ExperienceEnum? experience = null,
        DateTime? start = null,
        DateTime? end = null,
        List<Requirement>? requirements = null,
        List<string>? stack = null) => new()
    {
        Title = title,
        Description = description,
        Company = company,
        Salary = 10000,
        Category = category ?? CategoryA,
        Experience = experience ?? ExperienceA,
        StartDate = start ?? DateTime.UtcNow.AddDays(1),
        EndDate = end ?? DateTime.UtcNow.AddDays(30),
        Requirements = requirements,
        Stack = stack
    };

    public static OfferResponse Response(Guid? id = null, Guid? authorId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        AuthorId = authorId ?? Guid.NewGuid(),
        Title = "Backend dev",
        Description = "Opis",
        Company = "ACME",
        Salary = 10000,
        Category = CategoryA,
        Experience = ExperienceA,
        Stack = new List<string>(),
        Requirements = new List<Requirement>(),
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(30),
        CreatedOn = DateTime.UtcNow
    };

    public static async Task<JobOffer> SeedOfferAsync(
        OfferDbContext db,
        Guid? authorId = null,
        string title = "Seeded",
        DateTime? start = null,
        DateTime? end = null,
        DateTime? createdOn = null,
        CategoryEnum? category = null,
        ExperienceEnum? experience = null,
        int salary = 10000)
    {
        var offer = new JobOffer
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId ?? Guid.NewGuid(),
            Title = title,
            Description = "Opis",
            Company = "ACME",
            Salary = salary,
            Category = category ?? CategoryA,
            Experience = experience ?? ExperienceA,
            Stack = new List<string>(),
            Requirements = new List<Requirement>(),
            StartDate = start ?? DateTime.UtcNow.AddDays(1),
            EndDate = end ?? DateTime.UtcNow.AddDays(30),
            CreatedOn = createdOn ?? DateTime.UtcNow
        };

        db.Offers.Add(offer);
        await db.SaveChangesAsync();
        return offer;
    }
}