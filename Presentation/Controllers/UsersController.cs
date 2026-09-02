using Application.Services;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts;
using Presentation.Extensions;

namespace Presentation.Controllers;

/// <summary>
/// Управление пользователями: назначение/снятие роли администратора.
/// Права проверяются в сервисе (вызывающий админ).
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(UserService users) : ControllerBase
{
    /// <summary>Список всех пользователей для управления ролями (только админ).</summary>
    [HttpGet]
    public async Task<ActionResult<Page<UserResponse>>> List(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var page = await users.GetAllUsersAsync(
            User.GetUserId(),
            new QueryParams(cursor, pageSize),
            cancellationToken);

        var mapped = new Page<UserResponse>(
            page.Items.Select(UserResponse.From).ToList(),
            page.NextCursor);

        return Ok(mapped);
    }

    /// <summary>Назначает пользователю роль администратора (только админ).</summary>
    [HttpPost("{id:guid}/admin")]
    public async Task<ActionResult<UserResponse>> GrantAdmin(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.SetAdminAsync(User.GetUserId(), id, isAdmin: true, cancellationToken);
        return Ok(UserResponse.From(user));
    }

    /// <summary>Снимает роль администратора (только админ).</summary>
    [HttpDelete("{id:guid}/admin")]
    public async Task<ActionResult<UserResponse>> RevokeAdmin(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.SetAdminAsync(User.GetUserId(), id, isAdmin: false, cancellationToken);
        return Ok(UserResponse.From(user));
    }
}