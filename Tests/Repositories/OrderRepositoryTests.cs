using Domain.Contracts;
using Domain.Models;
using Domain.Types;
using Infrastructure.DataBase.Repositories;

namespace Tests.Repositories;

public class OrderRepositoryTests(RepositoriesFixture fixture) : RepositoryTestBase(fixture)
{
    private async Task<Guid> AddUserAsync()
    {
        await using var db = CreateContext();
        var added = await new UserRepository(db).AddAsync(User.Create("user" + Guid.NewGuid().ToString("N"), "hash"));
        await db.SaveChangesAsync();
        return added.Id;
    }

    private async Task<Order> AddOrderAsync(Guid userId, string? recipientCity = null, Status? status = null)
    {
        await using var db = CreateContext();
        var repo = new OrderRepository(db);

        var added = await repo.AddAsync(
            Order.Create(userId, "СПб", recipientCity ?? "Москва", "ул. Ленина, 1", "ул. Гагарина, 2", 5));

        if (status is not null)
        {
            var trackable = await db.Orders.FindAsync(added.Id);
            db.Entry(trackable!).Property(o => o.Status).CurrentValue = status.Value;
        }

        await db.SaveChangesAsync();
        return added;
    }

    [Fact]
    public async Task GetByUserId_ReturnsOnlyUsersOrders_Descending()
    {
        await ClearTablesAsync();
        var userId = await AddUserAsync();
        var otherId = await AddUserAsync();
        await AddOrderAsync(otherId);

        var o1 = await AddOrderAsync(userId);
        var o2 = await AddOrderAsync(userId);

        await using var db = CreateContext();
        var page = await new OrderRepository(db)
            .GetByUserIdAsync(userId, new QueryParams(PageSize: 100));

        var ids = page.Items.Select(o => o.Id).ToArray();
        Assert.Equal([o2.Id, o1.Id], ids);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task GetByStatus_FiltersByStatus_Descending()
    {
        await ClearTablesAsync();
        var userId = await AddUserAsync();

        var inTransit1 = await AddOrderAsync(userId, status: Status.InTransit);
        await AddOrderAsync(userId, status: Status.New);
        var inTransit2 = await AddOrderAsync(userId, status: Status.InTransit);

        await using var db = CreateContext();
        var page = await new OrderRepository(db)
            .GetByStatusAsync(Status.InTransit, new QueryParams(PageSize: 100));

        var ids = page.Items.Select(o => o.Id).ToArray();
        Assert.Equal([inTransit2.Id, inTransit1.Id], ids);
    }

    [Fact]
    public async Task GetByRecipientCity_FiltersByCity_Ascending()
    {
        await ClearTablesAsync();
        var userId = await AddUserAsync();

        var a = await AddOrderAsync(userId, recipientCity: "Москва");
        var b = await AddOrderAsync(userId, recipientCity: "Москва");
        await AddOrderAsync(userId, recipientCity: "Питер");

        await using var db = CreateContext();
        var page = await new OrderRepository(db)
            .GetByRecipientCityAsync("Москва", new QueryParams(PageSize: 100));

        var ids = page.Items.Select(o => o.Id).ToArray();
        Assert.Equal([a.Id, b.Id], ids);
    }

    [Fact]
    public async Task GetAll_WithoutStatus_ReturnsAllOrders_Descending()
    {
        await ClearTablesAsync();
        var userId = await AddUserAsync();
        var o1 = await AddOrderAsync(userId);
        var o2 = await AddOrderAsync(userId, status: Status.InTransit);

        await using var db = CreateContext();
        var page = await new OrderRepository(db).GetAllAsync(new QueryParams(PageSize: 100));

        var ids = page.Items.Select(o => o.Id).ToArray();
        Assert.Equal([o2.Id, o1.Id], ids);
    }

    [Fact]
    public async Task GetAll_WithStatus_FiltersByStatus()
    {
        await ClearTablesAsync();
        var userId = await AddUserAsync();

        var inTransit = await AddOrderAsync(userId, status: Status.InTransit);
        await AddOrderAsync(userId, status: Status.New);

        await using var db = CreateContext();
        var page = await new OrderRepository(db).GetAllAsync(new QueryParams(PageSize: 100), Status.InTransit);

        Assert.Equal([inTransit.Id], page.Items.Select(o => o.Id).ToArray());
    }

    [Fact]
    public async Task GetByUserId_PaginatesOverAllOrders_WithoutDuplicates()
    {
        await ClearTablesAsync();
        var userId = await AddUserAsync();

        var created = new List<Order>();
        for (var i = 0; i < 5; i++)
        {
            created.Add(await AddOrderAsync(userId));
            await Task.Delay(1);
        }

        await using var db = CreateContext();
        var repo = new OrderRepository(db);

        var page1 = await repo.GetByUserIdAsync(userId, new QueryParams(PageSize: 2));
        var page2 = await repo.GetByUserIdAsync(userId, new QueryParams(Cursor: page1.NextCursor, PageSize: 2));
        var page3 = await repo.GetByUserIdAsync(userId, new QueryParams(Cursor: page2.NextCursor, PageSize: 2));

        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(2, page2.Items.Count);
        Assert.Single(page3.Items);
        Assert.NotNull(page1.NextCursor);
        Assert.NotNull(page2.NextCursor);
        Assert.Null(page3.NextCursor);

        var allIds = page1.Items.Concat(page2.Items).Concat(page3.Items).Select(o => o.Id).ToArray();
        Assert.Equal(created.Select(o => o.Id).OrderByDescending(id => id), allIds);
    }
}