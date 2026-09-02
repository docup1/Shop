using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts;
using Presentation.Extensions;

namespace Presentation.Controllers;

/// <summary>Регистрация, вход, обновление/завершение сессии, профиль текущего пользователя.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth, UserService users) : ControllerBase
{
    /// <summary>Создаёт пользователя и выдаёт токены.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(request.UserName, request.Password, cancellationToken);
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Вход по имени пользователя и паролю.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.LoginAsync(request.UserName, request.Password, cancellationToken);
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Выдаёт новый access-токен по активной сессии (refresh-токен = Id сессии).</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.RefreshAsync(request.RefreshToken, cancellationToken);
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Завершает переданную сессию текущего пользователя.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(User.GetUserId(), request.RefreshToken, cancellationToken);
        return NoContent();
    }

    /// <summary>Профиль текущего пользователя.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await users.GetProfileAsync(User.GetUserId(), cancellationToken);
        return Ok(UserResponse.From(user));
    }
}