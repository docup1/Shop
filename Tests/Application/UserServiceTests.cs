using Application.Exceptions;
using Domain.Contracts;

namespace Tests.Application;

public class UserServiceTests
{
    private readonly ServiceTestHarness _harness = new();

    [Fact]
    public async Task GetProfile_KnownUser_ReturnsProfile()
    {
        var user = _harness.AddUser("alice", isAdmin: true);

        var profile = await _harness.UserService().GetProfileAsync(user.Id);

        Assert.Equal(user.Id, profile.Id);
        Assert.Equal(user.UserName, profile.UserName);
        Assert.True(profile.IsAdmin);
    }

    [Fact]
    public async Task GetProfile_UnknownUser_ThrowsNotFound()
        => await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.UserService().GetProfileAsync(Guid.NewGuid()));

    [Fact]
    public async Task GetAllUsers_ByAdmin_ReturnsAllUsers()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        _harness.AddUser("alice");
        _harness.AddUser("bob");

        var page = await _harness.UserService()
            .GetAllUsersAsync(admin.Id, new QueryParams());

        Assert.Equal(3, page.Items.Count);
    }

    [Fact]
    public async Task GetAllUsers_ByRegularUser_ThrowsUnauthorized()
    {
        var user = _harness.AddUser("alice");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _harness.UserService().GetAllUsersAsync(user.Id, new QueryParams()));
    }

    [Fact]
    public async Task SetAdmin_ByAdmin_GrantsRole_PreservesOtherFields()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        var target = _harness.AddUser("alice");

        var updated = await _harness.UserService().SetAdminAsync(admin.Id, target.Id, isAdmin: true);

        Assert.True(updated.IsAdmin);
        Assert.Equal(target.UserName, updated.UserName);
        Assert.Equal(target.PasswordHash, updated.PasswordHash);
        Assert.Equal(1, _harness.Uow.SaveCount);
        Assert.True(Assert.Single(_harness.Users.Items, u => u.Id == target.Id).IsAdmin);
    }

    [Fact]
    public async Task SetAdmin_ByAdmin_RevokesRole()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);
        var target = _harness.AddUser("alice", isAdmin: true);
        var service = _harness.UserService();

        var updated = await service.SetAdminAsync(admin.Id, target.Id, isAdmin: false);

        Assert.False(updated.IsAdmin);
        Assert.False(Assert.Single(_harness.Users.Items, u => u.Id == target.Id).IsAdmin);
    }

    [Fact]
    public async Task SetAdmin_ByRegularUser_ThrowsUnauthorized()
    {
        var user = _harness.AddUser("alice");
        var target = _harness.AddUser("bob");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _harness.UserService().SetAdminAsync(user.Id, target.Id, isAdmin: true));
    }

    [Fact]
    public async Task SetAdmin_CallerMissing_ThrowsNotFound()
        => await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.UserService().SetAdminAsync(Guid.NewGuid(), Guid.NewGuid(), isAdmin: true));

    [Fact]
    public async Task SetAdmin_TargetMissing_ThrowsNotFound()
    {
        var admin = _harness.AddUser("admin", isAdmin: true);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.UserService().SetAdminAsync(admin.Id, Guid.NewGuid(), isAdmin: true));
    }
}