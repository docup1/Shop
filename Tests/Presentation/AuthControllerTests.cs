using Application;
using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts;

namespace Tests.Presentation;

public class AuthControllerTests
{
    private readonly ControllerTestContext _ctx = new();
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Register_ReturnsTokenPair()
    {
        var result = (OkObjectResult)(await _ctx.AuthController()
            .Register(new RegisterRequest("alice", "password123"), CancellationToken.None)).Result!;

        var response = Assert.IsType<AuthResponse>(result.Value);
        var user = Assert.Single(_ctx.Harness.Users.Items);
        var session = Assert.Single(_ctx.Harness.Sessions.Items);

        Assert.Equal($"{user.Id}:{session.Id}", response.AccessToken);
        Assert.Equal(session.Id.ToString(), response.RefreshToken);
        Assert.Equal(session.ExpiresAt, response.ExpiresAt);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenPair()
    {
        var auth = _ctx.AuthController();
        await auth.Register(new RegisterRequest("alice", "password123"), CancellationToken.None);

        var result = (OkObjectResult)(await _ctx.AuthController()
            .Login(new LoginRequest("alice", "password123"), CancellationToken.None)).Result!;

        var response = Assert.IsType<AuthResponse>(result.Value);
        var session = _ctx.Harness.Sessions.Items[^1];
        Assert.Equal(session.Id.ToString(), response.RefreshToken);
    }

    [Fact]
    public async Task Login_BadCredentials_ThrowsUnauthorized()
    {
        var auth = _ctx.AuthController();
        await auth.Register(new RegisterRequest("alice", "password123"), CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _ctx.AuthController().Login(new LoginRequest("alice", "wrongpass"), CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_ReturnsNewAccessToken()
    {
        var user = _ctx.Harness.AddUser();
        var session = _ctx.Harness.AddSession(user.Id, Now.UtcDateTime.AddDays(30));

        var result = (OkObjectResult)(await _ctx.AuthController()
            .Refresh(new RefreshRequest(session.Id), CancellationToken.None)).Result!;

        var response = Assert.IsType<AuthResponse>(result.Value);
        Assert.Equal($"{user.Id}:{session.Id}", response.AccessToken);
        Assert.Equal(session.Id.ToString(), response.RefreshToken);
    }

    [Fact]
    public async Task Logout_RemovesOwnSession_ReturnsNoContent()
    {
        var user = _ctx.Harness.AddUser();
        var session = _ctx.Harness.AddSession(user.Id, Now.UtcDateTime.AddDays(30));

        var result = await _ctx.AuthController(user.Id)
            .Logout(new LogoutRequest(session.Id), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(_ctx.Harness.Sessions.Items);
    }

    [Fact]
    public async Task Me_ReturnsCallerProfile()
    {
        var user = _ctx.Harness.AddUser("alice", isAdmin: true);

        var result = (OkObjectResult)(await _ctx.AuthController(user.Id)
            .Me(CancellationToken.None)).Result!;

        var response = Assert.IsType<UserResponse>(result.Value);
        Assert.Equal(user.Id, response.Id);
        Assert.Equal("alice", response.UserName);
        Assert.True(response.IsAdmin);
    }

    [Fact]
    public async Task Me_MissingSubjectClaim_ThrowsUnauthorized()
        => await Assert.ThrowsAsync<UnauthorizedException>(
            () => _ctx.AuthController().Me(CancellationToken.None));
}