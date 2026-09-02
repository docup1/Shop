using Application.Exceptions;
using Domain.Contracts;
using Domain.Types;

namespace Tests.Application;

public class OrderServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly ServiceTestHarness _harness = new(Now);

    private async Task<Domain.Models.Order> CreateOrderAsync() => await _harness.OrderService()
        .CreateOrderAsync(_harness.AddUser("alice").Id, "СПб", "Москва", "ул. Ленина, 1", "ул. Гагарина, 2", 5);

    [Fact]
    public async Task CreateOrder_AddsOrderForCaller_WithNewStatus()
    {
        var caller = _harness.AddUser("alice");
        var service = _harness.OrderService();

        var order = await service.CreateOrderAsync(
            caller.Id, "СПб", "Москва", "ул. Ленина, 1", "ул. Гагарина, 2", 5);

        Assert.Equal(caller.Id, order.UserId);
        Assert.Equal(Status.New, order.Status);
        Assert.Equal(1, _harness.Uow.SaveCount);
        Assert.Same(order, Assert.Single(_harness.Orders.Items));
    }

    [Fact]
    public async Task CreateOrder_UnknownCaller_ThrowsNotFound()
        => await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.OrderService().CreateOrderAsync(
                Guid.NewGuid(), "СПб", "Москва", "ул. Ленина, 1", "ул. Гагарина, 2", 5));

    [Fact]
    public async Task GetOrders_AsRegularUser_ReturnsOnlyOwnOrders()
    {
        var alice = _harness.AddUser("alice");
        _harness.AddUser("bob");
        _harness.AddOrder(alice.Id, Status.InTransit);
        _harness.AddOrder(Guid.NewGuid(), Status.New);
        _harness.AddOrder(alice.Id, Status.New);

        var page = await _harness.OrderService()
            .GetOrdersAsync(alice.Id, new QueryParams(), status: Status.InTransit);

        Assert.All(page.Items, o => Assert.Equal(alice.Id, o.UserId));
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task GetOrders_AsAdmin_WithStatus_ReturnsFilteredAll()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        _harness.AddOrder(Guid.NewGuid(), Status.InTransit);
        _harness.AddOrder(Guid.NewGuid(), Status.New);
        _harness.AddOrder(Guid.NewGuid(), Status.InTransit);

        var page = await _harness.OrderService()
            .GetOrdersAsync(admin.Id, new QueryParams(), status: Status.InTransit);

        Assert.Equal(2, page.Items.Count);
        Assert.All(page.Items, o => Assert.Equal(Status.InTransit, o.Status));
    }

    [Fact]
    public async Task GetOrders_AsAdmin_WithoutStatus_ThrowsValidation()
        => await Assert.ThrowsAsync<ValidationException>(
            () => _harness.OrderService().GetOrdersAsync(
                _harness.AddUser("admin", isAdmin: true).Id, new QueryParams()));

    [Fact]
    public async Task GetOrder_AsOwner_ReturnsOrder()
    {
        var owner = _harness.AddUser("alice");
        var order = _harness.AddOrder(owner.Id, Status.InProgress);

        var fetched = await _harness.OrderService().GetOrderAsync(owner.Id, order.Id);

        Assert.Equal(order.Id, fetched.Id);
    }

    [Fact]
    public async Task GetOrder_AsOtherRegularUser_ThrowsNotFound()
    {
        var alice = _harness.AddUser("alice");
        var bob = _harness.AddUser("bob");
        var order = _harness.AddOrder(alice.Id);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.OrderService().GetOrderAsync(bob.Id, order.Id));
    }

    [Fact]
    public async Task GetOrder_AsAdmin_ReturnsAnyOrder()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        var order = _harness.AddOrder(Guid.NewGuid());

        var fetched = await _harness.OrderService().GetOrderAsync(admin.Id, order.Id);

        Assert.Equal(order.Id, fetched.Id);
    }

    [Fact]
    public async Task GetOrder_Missing_ThrowsNotFound()
        => await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.OrderService().GetOrderAsync(Guid.NewGuid(), Guid.NewGuid()));

    [Fact]
    public async Task ChangeStatus_ByAdmin_AdvancesStatus_Persists()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        var order = _harness.AddOrder(Guid.NewGuid(), Status.New);
        var service = _harness.OrderService();

        var updated = await service.ChangeStatusAsync(admin.Id, order.Id, Status.InProgress);

        Assert.Equal(Status.InProgress, updated.Status);
        Assert.Equal(Status.InProgress, Assert.Single(_harness.Orders.Items).Status);
        Assert.Equal(1, _harness.Uow.SaveCount);
    }

    [Fact]
    public async Task ChangeStatus_ByAdmin_SameStatus_IsNoop()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        var order = _harness.AddOrder(Guid.NewGuid(), Status.InProgress);

        var updated = await _harness.OrderService().ChangeStatusAsync(admin.Id, order.Id, Status.InProgress);

        Assert.Equal(Status.InProgress, updated.Status);
    }

    [Fact]
    public async Task ChangeStatus_ByRegularUser_ThrowsUnauthorized()
    {
        var user = _harness.AddUser("alice");
        var order = _harness.AddOrder(user.Id, Status.New);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _harness.OrderService().ChangeStatusAsync(user.Id, order.Id, Status.InProgress));
    }

    [Fact]
    public async Task ChangeStatus_InvalidTransition_ThrowsValidation()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        var order = _harness.AddOrder(Guid.NewGuid(), Status.New);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _harness.OrderService().ChangeStatusAsync(admin.Id, order.Id, Status.Delivered));

        Assert.Contains("New -> Delivered", ex.Message);
        Assert.Equal(Status.New, Assert.Single(_harness.Orders.Items).Status);
    }

    [Fact]
    public async Task ChangeStatus_MissingOrder_ThrowsNotFound()
        => await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.OrderService().ChangeStatusAsync(
                _harness.AddUser("admin", isAdmin: true).Id, Guid.NewGuid(), Status.InProgress));
}