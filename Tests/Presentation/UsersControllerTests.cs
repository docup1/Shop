using Application.Exceptions;
using Domain.Contracts;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts;

namespace Tests.Presentation;

public class UsersControllerTests
{
    private readonly ControllerTestContext _ctx = new();

    [Fact]
    public async Task List_ByAdmin_ReturnsAllUsers()
    {
        var admin = _ctx.Harness.AddUser("admin", isAdmin: true);
        _ctx.Harness.AddUser("alice");

        var result = (OkObjectResult)(await _ctx.UsersController(admin.Id)
            .List(cursor: null, pageSize: 20, CancellationToken.None)).Result!;

        var page = Assert.IsType<Page<UserResponse>>(result.Value);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task List_ByRegularUser_ThrowsUnauthorized()
    {
        var user = _ctx.Harness.AddUser("alice");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _ctx.UsersController(user.Id).List(cursor: null, pageSize: 20, CancellationToken.None));
    }

    [Fact]
    public async Task GrantAdmin_ByAdmin_ReturnsUserWithRole()
    {
        var admin = _ctx.Harness.AddUser("admin", isAdmin: true);
        var target = _ctx.Harness.AddUser("alice");

        var result = (OkObjectResult)(await _ctx.UsersController(admin.Id)
            .GrantAdmin(target.Id, CancellationToken.None)).Result!;

        var response = Assert.IsType<UserResponse>(result.Value);
        Assert.Equal(target.Id, response.Id);
        Assert.True(response.IsAdmin);
        Assert.True(Assert.Single(_ctx.Harness.Users.Items, u => u.Id == target.Id).IsAdmin);
    }

    [Fact]
    public async Task RevokeAdmin_ByAdmin_ReturnsUserWithoutRole()
    {
        var admin = _ctx.Harness.AddUser("admin", isAdmin: true);
        var target = _ctx.Harness.AddUser("alice", isAdmin: true);

        var result = (OkObjectResult)(await _ctx.UsersController(admin.Id)
            .RevokeAdmin(target.Id, CancellationToken.None)).Result!;

        var response = Assert.IsType<UserResponse>(result.Value);
        Assert.False(response.IsAdmin);
        Assert.False(Assert.Single(_ctx.Harness.Users.Items, u => u.Id == target.Id).IsAdmin);
    }

    [Fact]
    public async Task GrantAdmin_ByRegularUser_ThrowsUnauthorized()
    {
        var user = _ctx.Harness.AddUser("alice");
        var target = _ctx.Harness.AddUser("bob");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _ctx.UsersController(user.Id).GrantAdmin(target.Id, CancellationToken.None));
    }

    [Fact]
    public async Task GrantAdmin_WithoutSubjectClaim_ThrowsUnauthorized()
        => await Assert.ThrowsAsync<UnauthorizedException>(
            () => _ctx.UsersController().GrantAdmin(Guid.NewGuid(), CancellationToken.None));
}