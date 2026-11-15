using Microsoft.EntityFrameworkCore;
using Moq;
using User.Data;
using User.Models;
using User.Models.Enums;
using User.Services;

namespace User.Tests;

public class CvServiceTests
{
    private readonly Mock<IFileStorage> _storage = new();

    public CvServiceTests()
    {
        _storage.Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("stored.pdf");
    }

    private static UserDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private CvService CreateService(UserDbContext db) => new(db, _storage.Object);


    [Fact]
    public async Task UploadCv_Valid_StoresFileAndSavesPendingCv()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();

        var uploaded = await CreateService(db).UploadCvAsync(TestData.File("moje cv.pdf"), userId);

        Assert.True(uploaded);
        var cv = await db.CVs.SingleAsync();
        Assert.Equal(userId, cv.UserId);
        Assert.Equal("moje cv.pdf", cv.FileName);
        Assert.Equal("stored.pdf", cv.StorageKey);
        Assert.Equal(CvParseStatus.Pending, cv.ParseStatus);
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), ".pdf"), Times.Once);
    }

    [Fact]
    public async Task UploadCv_Docx_IsAccepted()
    {
        await using var db = CreateDb();

        var uploaded = await CreateService(db).UploadCvAsync(TestData.File("cv.DOCX", TestData.DocxBytes), Guid.NewGuid());

        Assert.True(uploaded);
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), ".docx"), Times.Once);
    }

    [Fact]
    public async Task UploadCv_NullFile_ReturnsFalse() =>
        await AssertUploadRejected(null);

    [Fact]
    public async Task UploadCv_EmptyFile_ReturnsFalse() =>
        await AssertUploadRejected(TestData.File(content: []));

    [Fact]
    public async Task UploadCv_TooLarge_ReturnsFalse() =>
        await AssertUploadRejected(TestData.File(content: new byte[5 * 1024 * 1024 + 1]));

    [Fact]
    public async Task UploadCv_WrongExtension_ReturnsFalse() =>
        await AssertUploadRejected(TestData.File("cv.txt"));

    private async Task AssertUploadRejected(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        await using var db = CreateDb();

        var uploaded = await CreateService(db).UploadCvAsync(file, Guid.NewGuid());

        Assert.False(uploaded);
        Assert.Equal(0, await db.CVs.CountAsync());
        _storage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }


    [Fact]
    public async Task GetCvs_ReturnsOnlyUsersActiveCvs_NewestFirst()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        await TestData.SeedCvAsync(db, userId, "stare.pdf", uploadedAt: DateTime.UtcNow.AddDays(-2));
        await TestData.SeedCvAsync(db, userId, "nowe.pdf", uploadedAt: DateTime.UtcNow);
        await TestData.SeedCvAsync(db, userId, "usuniete.pdf", deletedAt: DateTime.UtcNow);
        await TestData.SeedCvAsync(db, Guid.NewGuid(), "cudze.pdf");

        var result = await CreateService(db).GetCvsAsync(userId);

        Assert.Equal(["nowe.pdf", "stare.pdf"], result.Select(c => c.FileName));
    }

    [Fact]
    public async Task GetCvs_EmptyUserId_Throws()
    {
        await using var db = CreateDb();

        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(db).GetCvsAsync(Guid.Empty));
    }


    [Fact]
    public async Task GetCv_Completed_ReturnsParsedData()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var cv = await TestData.SeedCvAsync(db, userId, parsedData: new ParsedCvData { FullName = "Jan Kowalski" });

        var result = await CreateService(db).GetCvAsync(userId, cv.Id);

        Assert.Equal("Completed", result!.ParseStatus);
        Assert.Equal("Jan Kowalski", result.Data!.FullName);
    }

    [Fact]
    public async Task GetCv_NotParsedYet_ReturnsNoData()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var cv = await TestData.SeedCvAsync(db, userId, status: CvParseStatus.Pending);

        var result = await CreateService(db).GetCvAsync(userId, cv.Id);

        Assert.Equal("Pending", result!.ParseStatus);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetCv_OtherUsersCv_ReturnsNull()
    {
        await using var db = CreateDb();
        var cv = await TestData.SeedCvAsync(db, Guid.NewGuid());

        var result = await CreateService(db).GetCvAsync(Guid.NewGuid(), cv.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCv_Deleted_ReturnsNullUnlessIncludeDeleted()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var cv = await TestData.SeedCvAsync(db, userId, deletedAt: DateTime.UtcNow);
        var service = CreateService(db);

        Assert.Null(await service.GetCvAsync(userId, cv.Id));
        Assert.NotNull(await service.GetCvAsync(userId, cv.Id, includeDeleted: true));
    }


    [Fact]
    public async Task DeleteCv_Valid_SoftDeletesAndRemovesFile()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var cv = await TestData.SeedCvAsync(db, userId);

        var deleted = await CreateService(db).DeleteCvAsync(userId, cv.Id);

        Assert.True(deleted);
        Assert.NotNull((await db.CVs.FindAsync(cv.Id))!.DeletedAt);
        _storage.Verify(s => s.DeleteAsync(cv.StorageKey), Times.Once);
    }

    [Fact]
    public async Task DeleteCv_Pending_MarksParseAsFailed()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var cv = await TestData.SeedCvAsync(db, userId, status: CvParseStatus.Pending);

        await CreateService(db).DeleteCvAsync(userId, cv.Id);

        var saved = await db.CVs.FindAsync(cv.Id);
        Assert.Equal(CvParseStatus.Failed, saved!.ParseStatus);
        Assert.NotNull(saved.ParseError);
    }

    [Fact]
    public async Task DeleteCv_OtherUsersCv_ReturnsFalse()
    {
        await using var db = CreateDb();
        var cv = await TestData.SeedCvAsync(db, Guid.NewGuid());

        var deleted = await CreateService(db).DeleteCvAsync(Guid.NewGuid(), cv.Id);

        Assert.False(deleted);
        Assert.Null((await db.CVs.FindAsync(cv.Id))!.DeletedAt);
    }

    [Fact]
    public async Task DeleteCv_AlreadyDeleted_ReturnsFalse()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var cv = await TestData.SeedCvAsync(db, userId, deletedAt: DateTime.UtcNow);

        var deleted = await CreateService(db).DeleteCvAsync(userId, cv.Id);

        Assert.False(deleted);
        _storage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCv_EmptyIds_Throw()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteCvAsync(Guid.Empty, Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteCvAsync(Guid.NewGuid(), Guid.Empty));
    }
}
