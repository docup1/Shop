using Application.Exceptions;
using Domain.Contracts;
using Domain.Models;

namespace Application.Services;

/// <summary>
/// Регистрация и аутентификация: создание пользователя и сессии, вход, выход,
/// обновление access-токена по refresh-токену (Id сессии).
/// </summary>
public class AuthService(
    IUserRepository users,
    ISessionRepository sessions,
    IPasswordHasher passwordHasher,
    ITokenService tokens,
    IUnitOfWork uow,
    TimeSpan sessionLifetime)
{
    public const int MinPasswordLength = 8;

    public async Task<AuthResult> RegisterAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        ValidateCredentials(userName, password);

        if (await users.ExistsByUserNameAsync(userName, cancellationToken))
            throw new ConflictException($"User name '{userName}' is already taken.");

        var user = User.Create(userName, passwordHasher.Hash(password));
        await users.AddAsync(user, cancellationToken);

        return await IssueSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        ValidateCredentials(userName, password);

        var user = await users.GetByUserNameAsync(userName, cancellationToken);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("Invalid user name or password.");

        return await IssueSessionAsync(user, cancellationToken);
    }

    /// <summary>Завершает конкретную сессию текущего пользователя (logout).</summary>
    public async Task LogoutAsync(Guid callerUserId, Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetByIdAsync(refreshToken, cancellationToken);
        if (session is null || session.UserId != callerUserId)
            throw new NotFoundException("Session not found.");

        await sessions.RemoveAsync(session.Id, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Выдаёт новый access-токен по активной сессии (без ротации refresh).</summary>
    public async Task<AuthResult> RefreshAsync(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetActiveByIdAsync(refreshToken, cancellationToken);
        if (session is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await users.GetByIdAsync(session.UserId, cancellationToken);
        if (user is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        return new AuthResult(
            tokens.GenerateAccessToken(user.Id, user.UserName, session.Id),
            session.Id,
            session.ExpiresAt);
    }

    private async Task<AuthResult> IssueSessionAsync(User user, CancellationToken cancellationToken)
    {
        var session = Session.Create(user.Id, sessionLifetime);
        await sessions.AddAsync(session, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            tokens.GenerateAccessToken(user.Id, user.UserName, session.Id),
            session.Id,
            session.ExpiresAt);
    }

    private static void ValidateCredentials(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ValidationException("User name is required.", nameof(userName));
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            throw new ValidationException($"Password must be at least {MinPasswordLength} characters.", nameof(password));
    }
}