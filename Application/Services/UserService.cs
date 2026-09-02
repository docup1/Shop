using Application.Exceptions;
using Domain.Contracts;
using Domain.Models;

namespace Application.Services;

/// <summary>
/// Управление пользователями: назначение/снятие роли администратора.
/// </summary>
public class UserService(IUserRepository users, IUnitOfWork uow)
{
    /// <summary>Профиль текущего пользователя (для GET /api/auth/me).</summary>
    public async Task<User> GetProfileAsync(Guid callerUserId, CancellationToken cancellationToken = default)
        => await users.GetByIdAsync(callerUserId, cancellationToken)
           ?? throw new NotFoundException("User not found.");

    /// <summary>Список всех пользователей (только админ) для управления ролями.</summary>
    public async Task<Page<User>> GetAllUsersAsync(Guid callerUserId, QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var caller = await users.GetByIdAsync(callerUserId, cancellationToken)
            ?? throw new NotFoundException("Caller not found.");
        if (!caller.IsAdmin)
            throw new UnauthorizedException("Administrator privileges are required.");

        return await users.GetAllAsync(queryParams, cancellationToken);
    }

    public async Task<User> SetAdminAsync(Guid callerUserId, Guid targetUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var caller = await users.GetByIdAsync(callerUserId, cancellationToken)
            ?? throw new NotFoundException("Caller not found.");
        if (!caller.IsAdmin)
            throw new UnauthorizedException("Administrator privileges are required.");

        var target = await users.GetByIdAsync(targetUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var updated = target.SetAdmin(isAdmin);
        users.Update(updated);
        await uow.SaveChangesAsync(cancellationToken);

        return updated;
    }
}