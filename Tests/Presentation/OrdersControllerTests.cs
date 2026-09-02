using Application.Exceptions;
using Domain.Contracts;
using Domain.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts;
using Presentation.Controllers;

namespace Tests.Presentation;

public class OrdersControllerTests
{
    private readonly ControllerTestContext _ctx = new();

    [Fact]
    public async Task Create_ReturnsCreatedWithLocation_AndOrderBody()
    {
        var user = _ctx.Harness.AddUser();
        var controller = _ctx.OrdersController(user.Id);

        var result = (CreatedAtActionResult)(await controller.Create(
            new CreateOrderRequest("СПб", "Москва", "Невский пр., 1", "Тверская ул., 10", 5),
            CancellationToken.None)).Result!;

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Equal(nameof(OrdersController.GetById), result.ActionName);
        Assert.Equal(user.Id, ((OrderResponse)result.Value!).UserId);

        var order = Assert.Single(_ctx.Harness.Orders.Items);
        Assert.Equal("СПб", order.SenderCity);
        Assert.Equal(user.Id, order.UserId);
        Assert.Equal(Status.New, order.Status);
    }

    [Fact]
    public async Task List_RegularUser_ReturnsOnlyOwnOrders_WithNextCursor()
    {
        var me = _ctx.Harness.AddUser();
        var other = _ctx.Harness.AddUser("bob");
        var myOrder = _ctx.Harness.AddOrder(me.Id);
        _ctx.Harness.AddOrder(other.Id);

        var result = (OkObjectResult)(await _ctx.OrdersController(me.Id)
            .List(cursor: null, pageSize: 20, status: null, CancellationToken.None)).Result!;

        var page = Assert.IsType<Page<OrderResponse>>(result.Value);
        var item = Assert.Single(page.Items);
        Assert.Equal(myOrder.Id, item.Id);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task List_Admin_WithStatus_ReturnsCatalogOfThatStatus()
    {
        var admin = _ctx.Harness.AddUser("admin", isAdmin: true);
        var orderNew = _ctx.Harness.AddOrder(Guid.NewGuid(), Status.New);
        _ctx.Harness.AddOrder(Guid.NewGuid(), Status.InProgress);

        var result = (OkObjectResult)(await _ctx.OrdersController(admin.Id)
            .List(cursor: null, pageSize: 20, status: Status.New, CancellationToken.None)).Result!;

        var page = Assert.IsType<Page<OrderResponse>>(result.Value);
        Assert.Equal(orderNew.Id, Assert.Single(page.Items).Id);
    }

    [Fact]
    public async Task List_Admin_WithoutStatus_ThrowsValidation()
        => await Assert.ThrowsAsync<ValidationException>(
            () => _ctx.OrdersController(_ctx.Harness.AddUser("admin", isAdmin: true).Id)
                .List(cursor: null, pageSize: 20, status: null, CancellationToken.None));

    [Fact]
    public async Task GetById_OwnOrder_ReturnsOrder()
    {
        var user = _ctx.Harness.AddUser();
        var order = _ctx.Harness.AddOrder(user.Id, Status.InProgress);

        var result = (OkObjectResult)(await _ctx.OrdersController(user.Id)
            .GetById(order.Id, CancellationToken.None)).Result!;

        var response = Assert.IsType<OrderResponse>(result.Value);
        Assert.Equal(order.Id, response.Id);
        Assert.Equal(Status.InProgress, response.Status);
    }

    [Fact]
    public async Task GetById_AnotherUsersOrder_ThrowsNotFound()
    {
        var user = _ctx.Harness.AddUser();
        var order = _ctx.Harness.AddOrder(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(
            () => _ctx.OrdersController(user.Id).GetById(order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task GetById_Admin_SeesAnyOrder()
    {
        var admin = _ctx.Harness.AddUser("admin", isAdmin: true);
        var order = _ctx.Harness.AddOrder(Guid.NewGuid());

        var result = (OkObjectResult)(await _ctx.OrdersController(admin.Id)
            .GetById(order.Id, CancellationToken.None)).Result!;

        Assert.Equal(order.Id, Assert.IsType<OrderResponse>(result.Value).Id);
    }

    [Fact]
    public async Task ChangeStatus_ByAdmin_MovesOrderState()
    {
        var admin = _ctx.Harness.AddUser("admin", isAdmin: true);
        var order = _ctx.Harness.AddOrder(Guid.NewGuid(), Status.New);

        var result = (OkObjectResult)(await _ctx.OrdersController(admin.Id)
            .ChangeStatus(order.Id, new ChangeStatusRequest(Status.InProgress), CancellationToken.None)).Result!;

        Assert.Equal(Status.InProgress, Assert.IsType<OrderResponse>(result.Value).Status);
    }

    [Fact]
    public async Task ChangeStatus_ByRegularUser_ThrowsUnauthorized()
    {
        var user = _ctx.Harness.AddUser();
        var order = _ctx.Harness.AddOrder(user.Id, Status.New);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _ctx.OrdersController(user.Id)
                .ChangeStatus(order.Id, new ChangeStatusRequest(Status.InProgress), CancellationToken.None));
    }
}