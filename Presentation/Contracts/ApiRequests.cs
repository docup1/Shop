using System.ComponentModel.DataAnnotations;
using Domain.Types;

namespace Presentation.Contracts;

/// <summary>Тело POST /api/auth/register.</summary>
public sealed record RegisterRequest(
    [Required(AllowEmptyStrings = false)] string UserName,
    [Required(AllowEmptyStrings = false)] string Password);

/// <summary>Тело POST /api/auth/login.</summary>
public sealed record LoginRequest(
    [Required(AllowEmptyStrings = false)] string UserName,
    [Required(AllowEmptyStrings = false)] string Password);

/// <summary>Тело POST /api/auth/refresh. Refresh-токен — Id активной сессии.</summary>
public sealed record RefreshRequest(Guid RefreshToken);

/// <summary>Тело POST /api/auth/logout. Завершается именно эта сессия.</summary>
public sealed record LogoutRequest(Guid RefreshToken);

/// <summary>Тело POST /api/orders.</summary>
public sealed record CreateOrderRequest(
    [Required(AllowEmptyStrings = false)] string SenderCity,
    [Required(AllowEmptyStrings = false)] string RecipientCity,
    [Required(AllowEmptyStrings = false)] string SenderAddress,
    [Required(AllowEmptyStrings = false)] string RecipientAddress,
    [Range(1, int.MaxValue, ErrorMessage = "Weight must be greater than zero.")] int Weight);

/// <summary>Тело PATCH /api/orders/{id}/status.</summary>
public sealed record ChangeStatusRequest(Status Status);