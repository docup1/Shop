namespace Infrastructure.JWT;

/// <summary>
/// Настройки JWT из секции "Jwt" в appsettings.
/// SigningKey — секрет подписи HS256; в проде должен храниться вне репозитория
/// (env-переменная, User Secrets, Secret Manager).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKey { get; init; } = string.Empty;

    /// <summary>Срок жизни access-токена.</summary>
    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    /// <summary>Срок жизни сессии (refresh-токена). Передаётся в Session.Create.</summary>
    public int SessionLifetimeDays { get; init; } = 30;
}