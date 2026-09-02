namespace Domain.Contracts;

/// <summary>
/// Генерация access-токенов. Refresh-токеном служит Id строки Session
/// (таблица sessions уже в БД), поэтому интерфейс не знает о refresh.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string userName, Guid sessionId);
}