using System.Security.Claims;
using Application.Exceptions;
using System.IdentityModel.Tokens.Jwt;

namespace Presentation.Extensions;

/// <summary>Извлечение id текущего пользователя из claims access-токена.</summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// Возвращает userId из claim 'sub'. С дефолтным маппингом входящих claims
    /// JwtSecurityTokenHandler переименовывает 'sub' в ClaimTypes.NameIdentifier,
    /// поэтому проверяем оба варианта. Отсутствие/некорректный формат — 401.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (value is null || !Guid.TryParse(value, out var userId))
            throw new UnauthorizedException("Access token has no valid subject claim.");

        return userId;
    }
}