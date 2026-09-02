using Domain.Contracts;
using Domain.Models;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Repositories;
using Tests.Fakes;

namespace Tests.Repositories;

public class SessionRepositoryTests(RepositoriesFixture fixture) : RepositoryTestBase(fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTime NowUtc = Now.UtcDateTime;

    private static SessionRepository CreateRepository(ApplicationDbContext db)
        => new(db, new FakeTimeProvider(Now));

    private async Task<Guid> AddUserAsync(ApplicationDbContext db, string name)
    {
        var user = await new UserRepository(db).AddAsync(User.Create(name, "hash"));
        await db.SaveChangesAsync();
        return user.Id;
    }

    private async Task<Session> AddSessionAsync(Guid userId, DateTime expiresAt)
    {
        await using var db = CreateContext();
        var added = await CreateRepository(db).AddAsync(
            Session.Restore(Guid.CreateVersion7(), userId, expiresAt, NowUtc));
        await db.SaveChangesAsync();
        return added;
    }

    [Fact]
    public async Task GetActiveByUserId_ExcludesExpiredSessions()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var userId = await AddUserAsync(db, "alice");

        var active = await AddSessionAsync(userId, NowUtc.AddHours(1));
        await AddSessionAsync(userId, NowUtc.AddHours(-1));

        var page = await CreateRepository(db)
            .GetActiveByUserIdAsync(userId, new QueryParams(PageSize: 100));

        Assert.Equal([active.Id], page.Items.Select(s => s.Id).ToArray());
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task GetActiveByUserId_FiltersByUser_AndOrdersDescending()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var aliceId = await AddUserAsync(db, "alice");
        var bobId = await AddUserAsync(db, "bob");

        await AddSessionAsync(bobId, NowUtc.AddHours(1));
        await AddSessionAsync(aliceId, NowUtc.AddHours(1));
        await AddSessionAsync(aliceId, NowUtc.AddHours(1));
        await AddSessionAsync(aliceId, NowUtc.AddHours(1));

        var page = await CreateRepository(db)
            .GetActiveByUserIdAsync(aliceId, new QueryParams(PageSize: 100));

        var ids = page.Items.Select(s => s.Id).ToArray();
        Assert.Equal(3, ids.Length);
        Assert.All(page.Items, s => Assert.Equal(aliceId, s.UserId));
        Assert.True(IsStrictlyDescending(ids));
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task GetExpired_ReturnsOnlyExpired_NewestFirst()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var userId = await AddUserAsync(db, "alice");

        var e1 = await AddSessionAsync(userId, NowUtc.AddMinutes(-30));
        var e2 = await AddSessionAsync(userId, NowUtc.AddMinutes(-10));
        await AddSessionAsync(userId, NowUtc.AddHours(1));

        var page = await CreateRepository(db)
            .GetExpiredAsync(new QueryParams(PageSize: 100));

        var ids = page.Items.Select(s => s.Id).ToArray();
        Assert.Equal(2, ids.Length);
        Assert.Contains(e1.Id, ids);
        Assert.Contains(e2.Id, ids);
        Assert.True(IsStrictlyDescending(ids));
    }

    [Fact]
    public async Task GetActiveById_ReturnsActiveSession()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var userId = await AddUserAsync(db, "alice");
        var session = await AddSessionAsync(userId, NowUtc.AddHours(1));

        var fetched = await CreateRepository(db).GetActiveByIdAsync(session.Id);

        Assert.NotNull(fetched);
        Assert.Equal(session.Id, fetched.Id);
    }

    [Fact]
    public async Task GetActiveById_ReturnsNull_WhenExpiredOrMissing()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();
        var userId = await AddUserAsync(db, "alice");
        var expired = await AddSessionAsync(userId, NowUtc.AddHours(-1));

        var repo = CreateRepository(db);

        Assert.Null(await repo.GetActiveByIdAsync(expired.Id));
        Assert.Null(await repo.GetActiveByIdAsync(Guid.NewGuid()));
    }

    private static bool IsStrictlyDescending(IReadOnlyList<Guid> ids)
    {
        for (var i = 1; i < ids.Count; i++)
            if (ids[i - 1].CompareTo(ids[i]) <= 0)
                return false;
        return true;
    }
}