namespace Application;

/// <summary>
/// Результат аутентификации. <see cref="RefreshToken"/> — Id созданной сессии,
/// которым на фронтефей тейтся новый access-токен.
/// </summary>
public sealed record AuthResult(string AccessToken, Guid RefreshToken, DateTime ExpiresAt);