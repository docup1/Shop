using Domain.Contracts;
using Domain.Models;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Mapping;
using Infrastructure.DataBase.Repositories;
using Microsoft.EntityFrameworkCore;
using Entities = Infrastructure.DataBase.Entities;

namespace Tests.Repositories;

public class PagingTests(RepositoriesFixture fixture) : RepositoryTestBase(fixture)
{
    private static Task<Page<User>> PageAsync(
        IQueryable<Entities.User> source,
        QueryParams parameters)
        => Paging.ToPageAsync(
            source,
            parameters,
            EntityMapper.ToDomain,
            e => e.Id);

    private static IQueryable<Entities.User> AllUsers(ApplicationDbContext db)
        => db.Users.AsNoTracking().OrderBy(u => u.Id);

    [Fact]
    public async Task EmptySource_ReturnsEmptyPage()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();

        var page = await PageAsync(AllUsers(db), new QueryParams());

        Assert.Empty(page.Items);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task InvalidCursor_Throws()
    {
        await ClearTablesAsync();
        await using var db = CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            PageAsync(AllUsers(db), new QueryParams(Cursor: "not-a-guid")));
    }

    [Fact]
    public async Task PageSizeZero_IsClampedToOne()
    {
        await ClearTablesAsync();
        await using var setup = CreateContext();
        var repo = new UserRepository(setup);
        await repo.AddAsync(User.Create("a", "h"));
        await setup.SaveChangesAsync();
        await repo.AddAsync(User.Create("b", "h"));
        await setup.SaveChangesAsync();
        await repo.AddAsync(User.Create("c", "h"));
        await setup.SaveChangesAsync();

        await using var db = CreateContext();
        var page1 = await PageAsync(AllUsers(db), new QueryParams(PageSize: 0));
        var page2 = await PageAsync(AllUsers(db), new QueryParams(Cursor: page1.NextCursor, PageSize: 0));
        var page3 = await PageAsync(AllUsers(db), new QueryParams(Cursor: page2.NextCursor, PageSize: 0));

        Assert.Single(page1.Items);
        Assert.Single(page2.Items);
        Assert.Single(page3.Items);
        Assert.NotNull(page1.NextCursor);
        Assert.NotNull(page2.NextCursor);
        Assert.Null(page3.NextCursor);

        var visited = new[]
        {
            Assert.Single(page1.Items).Id,
            Assert.Single(page2.Items).Id,
            Assert.Single(page3.Items).Id
        };
        Assert.Equal(3, visited.Distinct().Count());
    }
}