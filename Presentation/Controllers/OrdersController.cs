using Application.Services;
using Domain.Contracts;
using Domain.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts;
using Presentation.Extensions;

namespace Presentation.Controllers;

/// <summary>
/// Заказы: создание и просмотр для всех аутентифицированных пользователей,
/// смена статусов и фильтр каталога — только для администраторов (проверяется в сервисе).
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(OrderService orders) : ControllerBase
{
    /// <summary>Создаёт заказ от имени текущего пользователя.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orders.CreateOrderAsync(
            User.GetUserId(),
            request.SenderCity,
            request.RecipientCity,
            request.SenderAddress,
            request.RecipientAddress,
            request.Weight,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, OrderResponse.From(order));
    }

    /// <summary>
    /// Страница заказов. Админ видит все заказы и обязан передать status;
    /// обычный пользователь — только свои (параметр status игнорируется).
    /// Пагинация cursor-based: передайте nextCursor из предыдущей страницы.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Page<OrderResponse>>> List(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20,
        [FromQuery] Status? status = null,
        CancellationToken cancellationToken = default)
    {
        var page = await orders.GetOrdersAsync(
            User.GetUserId(),
            new QueryParams(cursor, pageSize),
            status,
            cancellationToken);

        var mapped = new Page<OrderResponse>(
            page.Items.Select(OrderResponse.From).ToList(),
            page.NextCursor);

        return Ok(mapped);
    }

    /// <summary>Заказ по id. Доступен владельцу или администратору.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOrderAsync(User.GetUserId(), id, cancellationToken);
        return Ok(OrderResponse.From(order));
    }

    /// <summary>Переводит заказ в новый статус по стейт-машине (только админ).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderResponse>> ChangeStatus(Guid id, ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await orders.ChangeStatusAsync(User.GetUserId(), id, request.Status, cancellationToken);
        return Ok(OrderResponse.From(order));
    }
}