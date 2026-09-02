using Application;
using Domain.Models;
using Domain.Types;

namespace Presentation.Contracts;

/// <summary>
/// Результат аутентификации. <see cref="RefreshToken"/> — Id сессии (он же refresh-токен),
/// <see cref="ExpiresAt"/> — срок жизни сессии.
/// </summary>
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt)
{
    public static AuthResponse From(AuthResult result)
        => new(result.AccessToken, result.RefreshToken.ToString(), result.ExpiresAt);
}

/// <summary>Публичное представление пользователя (без хэша пароля).</summary>
public sealed record UserResponse(Guid Id, string UserName, bool IsAdmin)
{
    public static UserResponse From(User user) => new(user.Id, user.UserName, user.IsAdmin);
}

/// <summary>Публичное представление заказа.</summary>
public sealed record OrderResponse(
    Guid Id,
    Guid UserId,
    string SenderCity,
    string RecipientCity,
    string SenderAddress,
    string RecipientAddress,
    int Weight,
    Status Status,
    DateTime CreatedAt)
{
    public static OrderResponse From(Order order)
        => new(
            order.Id,
            order.UserId,
            order.SenderCity,
            order.RecipientCity,
            order.SenderAddress,
            order.RecipientAddress,
            order.Weight,
            order.Status,
            order.CreatedAt);
}