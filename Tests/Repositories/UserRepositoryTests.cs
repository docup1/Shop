using Domain.Contracts;
using Domain.Models;
using Infrastructure.DataBase.Repositories;

namespace Tests.Repositories;

public class UserRepositoryTests(RepositoriesFixture fixture) : RepositoryTestBase(fixture)
{
    private static readonly string[] UserNames =
    [
        "admin1",
        "admin2",
        "user1",
        "user2",
        "user3"
    ];

    [Fact]
    public async Task Add_And_GetById_RoundTrips()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var added = await repo.AddAsync(User.Create("alice", "hash"));
        await db.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(added.Id);

        Assert.NotNull(fetched);
        Assert.Equal(added.Id, fetched.Id);
        Assert.Equal("alice", fetched.UserName);
        Assert.Equal("hash", fetched.PasswordHash);
        Assert.False(fetched.IsAdmin);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNull()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();

        Assert.Null(await new UserRepository(db).GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsById_ReflectsPresence()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var added = await repo.AddAsync(User.Create("alice", "hash"));
        await db.SaveChangesAsync();

        Assert.True(await repo.ExistsByIdAsync(added.Id));
        Assert.False(await repo.ExistsByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByUserName_FindsUser()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var added = await repo.AddAsync(User.Create("alice", "hash"));
        await db.SaveChangesAsync();

        var fetched = await repo.GetByUserNameAsync("alice");

        Assert.NotNull(fetched);
        Assert.Equal(added.Id, fetched.Id);

        Assert.Null(await repo.GetByUserNameAsync("missing"));
    }

    [Fact]
    public async Task ExistsByUserName_ReflectsPresence()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        await repo.AddAsync(User.Create("alice", "hash"));
        await db.SaveChangesAsync();

        Assert.True(await repo.ExistsByUserNameAsync("alice"));
        Assert.False(await repo.ExistsByUserNameAsync("bob"));
    }

    [Fact]
    public async Task Update_PersistsNewValues()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var added = await repo.AddAsync(User.Create("alice", "hash"));
        await db.SaveChangesAsync();

        await using var updateDb = CreateContext();
        var updateRepo = new UserRepository(updateDb);
        updateRepo.Update(User.Restore(added.Id, "alice2", "hash2", isAdmin: true));
        await updateDb.SaveChangesAsync();

        await using var readDb = CreateContext();
        var fetched = await new UserRepository(readDb).GetByIdAsync(added.Id);

        Assert.Equal("alice2", fetched!.UserName);
        Assert.Equal("hash2", fetched.PasswordHash);
        Assert.True(fetched.IsAdmin);
    }

    [Fact]
    public async Task Remove_DeletesExisting_ReturnsFalseWhenMissing()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var added = await repo.AddAsync(User.Create("alice", "hash"));
        await db.SaveChangesAsync();

        Assert.True(await repo.RemoveAsync(added.Id));
        Assert.False(await repo.ExistsByIdAsync(added.Id));
        Assert.False(await repo.RemoveAsync(added.Id));
    }

    [Fact]
    public async Task GetAdmins_Paginates_WithoutDuplicates()
    {
        await ClearTablesAsync();
        await using var setup = CreateContext();
        var users = new List<User>();
        foreach (var name in UserNames)
        {
            var repo = new UserRepository(setup);
            var added = await repo.AddAsync(User.Create(name, "hash", isAdmin: name.StartsWith("admin")));
            await setup.SaveChangesAsync();
            users.Add(added);
        }

        var adminIds = users.Where(u => u.IsAdmin).Select(u => u.Id).ToList();

        await using var db = CreateContext();
        var repo2 = new UserRepository(db);

        var page1 = await repo2.GetAdminsAsync(new QueryParams(PageSize: 1));

        var item1 = Assert.Single(page1.Items);
        Assert.Contains(item1.Id, adminIds);
        Assert.NotNull(page1.NextCursor);

        var page2 = await repo2.GetAdminsAsync(new QueryParams(Cursor: page1.NextCursor, PageSize: 1));

        var item2 = Assert.Single(page2.Items);
        Assert.Contains(item2.Id, adminIds);
        Assert.Null(page2.NextCursor);

        Assert.NotEqual(item1.Id, item2.Id);
    }
}