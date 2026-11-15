using Microsoft.EntityFrameworkCore;
using User.Data;
using User.Models.Enums;
using User.Services;

namespace User.Tests;

public class UserServiceTests
{
    private static UserDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);


    [Fact]
    public async Task CreateUser_New_SavesProfileAndReturnsTrue()
    {
        await using var db = CreateDb();
        var id = Guid.NewGuid();

        var created = await new UserService(db).CreateUserAsync(id);

        Assert.True(created);
        Assert.NotNull(await db.Users.FindAsync(id));
    }

    [Fact]
    public async Task CreateUser_AlreadyExists_ReturnsFalse()
    {
        await using var db = CreateDb();
        var seeded = await TestData.SeedUserAsync(db);

        var created = await new UserService(db).CreateUserAsync(seeded.Id);

        Assert.False(created);
        Assert.Equal(1, await db.Users.CountAsync());
    }


    [Fact]
    public async Task GetAllUsers_ReturnsEveryProfile()
    {
        await using var db = CreateDb();
        await TestData.SeedUserAsync(db, firstname: "Anna");
        await TestData.SeedUserAsync(db, firstname: "Jan");

        var result = await new UserService(db).GetAllUsersAsync();

        Assert.Equal(["Anna", "Jan"], result.Select(u => u.Firstname).Order());
    }

    [Fact]
    public async Task GetUser_Existing_ReturnsProfile()
    {
        await using var db = CreateDb();
        var seeded = await TestData.SeedUserAsync(db);

        var result = await new UserService(db).GetUserAsync(seeded.Id);

        Assert.NotNull(result);
        Assert.Equal(seeded.Id, result!.Id);
        Assert.Equal("Jan", result.Firstname);
    }

    [Fact]
    public async Task GetUser_Missing_ReturnsNull()
    {
        await using var db = CreateDb();

        var result = await new UserService(db).GetUserAsync(Guid.NewGuid());

        Assert.Null(result);
    }


    [Fact]
    public async Task UpdateUser_Existing_UpdatesAndTrimsFields()
    {
        await using var db = CreateDb();
        var seeded = await TestData.SeedUserAsync(db);

        var result = await new UserService(db).UpdateUserAsync(seeded.Id, TestData.UpdateRequest(
            firstname: "  Anna ", city: " Kraków ", stack: ["Go", "SQL"], experience: Experience.Senior));

        Assert.Equal("Anna", result!.Firstname);
        Assert.Equal("Kraków", result.City);
        Assert.Equal(["Go", "SQL"], result.Stack);
        Assert.Equal(Experience.Senior, result.Experience);
    }

    [Fact]
    public async Task UpdateUser_Missing_CreatesProfile()
    {
        await using var db = CreateDb();
        var id = Guid.NewGuid();

        var result = await new UserService(db).UpdateUserAsync(id, TestData.UpdateRequest());

        Assert.Equal(id, result!.Id);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task UpdateUser_NullFields_ClearsThem()
    {
        await using var db = CreateDb();
        var seeded = await TestData.SeedUserAsync(db);

        var result = await new UserService(db).UpdateUserAsync(seeded.Id, TestData.UpdateRequest(firstname: null));

        Assert.Null(result!.Firstname);
    }
}
