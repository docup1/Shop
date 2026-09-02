using Application;
using Application.Exceptions;

namespace Tests.Application;

public class AuthServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly ServiceTestHarness _harness = new(Now);

    [Fact]
    public async Task Register_CreatesStoredUserAndSession_ReturnsTokenPair()
    {
        var service = _harness.Auth();

        var result = await service.RegisterAsync("alice", "password123");

        var user = Assert.Single(_harness.Users.Items);
        Assert.Equal("alice", user.UserName);
        Assert.Equal("hash:password123", user.PasswordHash);
        Assert.False(user.IsAdmin);
        Assert.Equal(1, _harness.Uow.SaveCount);

        var session = Assert.Single(_harness.Sessions.Items);
        Assert.Equal(user.Id, session.UserId);
        Assert.Equal(session.Id, result.RefreshToken);
        Assert.Equal($"{user.Id}:{session.Id}", result.AccessToken);
        Assert.Equal(session.ExpiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task Register_UniqueId_SameName_Twice_ThrowsConflict()
    {
        var service = _harness.Auth();

        await service.RegisterAsync("alice", "password123");

        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync("alice", "password456"));
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("")]
    public async Task Register_WeakPassword_ThrowsValidation(string password)
        => await Assert.ThrowsAsync<ValidationException>(
            () => _harness.Auth().RegisterAsync("alice", password));

    [Fact]
    public async Task Register_MissingUserName_ThrowsValidation()
        => await Assert.ThrowsAsync<ValidationException>(
            () => _harness.Auth().RegisterAsync("  ", "password123"));

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenPair()
    {
        var auth = _harness.Auth();
        await auth.RegisterAsync("alice", "password123");

        var result = await auth.LoginAsync("alice", "password123");

        var user = Assert.Single(_harness.Users.Items);
        Assert.Equal(2, _harness.Sessions.Items.Count);
        var session = _harness.Sessions.Items[^1];
        Assert.Equal(session.Id, result.RefreshToken);
        Assert.Equal($"{user.Id}:{session.Id}", result.AccessToken);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        var auth = _harness.Auth();
        await auth.RegisterAsync("alice", "password123");

        await Assert.ThrowsAsync<UnauthorizedException>(() => auth.LoginAsync("alice", "wrongpass"));
    }

    [Fact]
    public async Task Login_UnknownUser_ThrowsUnauthorized()
        => await Assert.ThrowsAsync<UnauthorizedException>(
            () => _harness.Auth().LoginAsync("ghost", "password123"));

    [Fact]
    public async Task Logout_RemovesOwnSession()
    {
        var auth = _harness.Auth();
        var session = _harness.AddSession(_harness.AddUser().Id, Now.UtcDateTime.AddDays(30));

        await auth.LogoutAsync(session.UserId, session.Id);

        Assert.Empty(_harness.Sessions.Items);
        Assert.Equal(1, _harness.Uow.SaveCount);
    }

    [Fact]
    public async Task Logout_AnotherUsersSession_ThrowsNotFound_AndKeepsSession()
    {
        var owner = _harness.AddUser("bob");
        var session = _harness.AddSession(owner.Id, Now.UtcDateTime.AddDays(30));

        var auth = _harness.Auth();
        await Assert.ThrowsAsync<NotFoundException>(() => auth.LogoutAsync(Guid.NewGuid(), session.Id));

        Assert.Single(_harness.Sessions.Items);
    }

    [Fact]
    public async Task Logout_MissingSession_ThrowsNotFound()
        => await Assert.ThrowsAsync<NotFoundException>(
            () => _harness.Auth().LogoutAsync(Guid.NewGuid(), Guid.NewGuid()));

    [Fact]
    public async Task Refresh_ActiveSession_ReturnsNewAccessToken()
    {
        var user = _harness.AddUser();
        _harness.AddSession(user.Id, Now.UtcDateTime.AddDays(30));

        var result = await _harness.Auth().RefreshAsync(_harness.Sessions.Items.Single().Id);

        var session = _harness.Sessions.Items.Single();
        Assert.Equal($"{user.Id}:{session.Id}", result.AccessToken);
        Assert.Equal(session.Id, result.RefreshToken);
    }

    [Fact]
    public async Task Refresh_ExpiredSession_ThrowsUnauthorized()
    {
        _harness.AddSession(_harness.AddUser().Id, Now.UtcDateTime.AddMinutes(-1));

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _harness.Auth().RefreshAsync(_harness.Sessions.Items.Single().Id));
    }

    [Fact]
    public async Task Refresh_UnknownSession_ThrowsUnauthorized()
        => await Assert.ThrowsAsync<UnauthorizedException>(
            () => _harness.Auth().RefreshAsync(Guid.NewGuid()));

    [Fact]
    public async Task Refresh_SessionOfDeletedUser_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid();
        _harness.AddSession(userId, Now.UtcDateTime.AddDays(30));

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _harness.Auth().RefreshAsync(_harness.Sessions.Items.Single().Id));
    }
}