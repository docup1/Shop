using Application.Exceptions;
using Domain.Contracts;
using Domain.Models;
using Domain.Types;

namespace Application.Services;

/// <summary>
/// Заказы: создание и просмотр для всех аутентифицированных, смена статусов и
/// фильтр каталога — только для администраторов.
/// </summary>
public class OrderService(IUserRepository users, IOrderRepository orders, IUnitOfWork uow)
{
    public async Task<Order> CreateOrderAsync(Guid callerUserId, string senderCity, string recipientCity,
        string senderAddress, string recipientAddress, int weight, CancellationToken cancellationToken = default)
    {
        await GetCallerAsync(callerUserId, requireAdmin: false, cancellationToken);

        var order = Order.Create(callerUserId, senderCity, recipientCity, senderAddress, recipientAddress, weight);
        await orders.AddAsync(order, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return order;
    }

    /// <summary>
    /// Страница заказов. Админ видит все заказы (с обязательным фильтром по статусу),
    /// обычный пользователь — только свои.
    /// </summary>
    public async Task<Page<Order>> GetOrdersAsync(Guid callerUserId, QueryParams queryParams, Status? status = null,
        CancellationToken cancellationToken = default)
    {
        var caller = await GetCallerAsync(callerUserId, requireAdmin: false, cancellationToken);

        if (caller.IsAdmin)
        {
            if (status is null)
                throw new ValidationException("Status filter is required for the admin order catalog.", nameof(status));

            return await orders.GetAllAsync(queryParams, status, cancellationToken);
        }

        return await orders.GetByUserIdAsync(callerUserId, queryParams, cancellationToken);
    }

    public async Task<Order> GetOrderAsync(Guid callerUserId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var caller = await GetCallerAsync(callerUserId, requireAdmin: false, cancellationToken);
        var order = await orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!caller.IsAdmin && order.UserId != callerUserId)
            throw new NotFoundException("Order not found.");

        return order;
    }

    public async Task<Order> ChangeStatusAsync(Guid callerUserId, Guid orderId, Status newStatus, CancellationToken cancellationToken = default)
    {
        await GetCallerAsync(callerUserId, requireAdmin: true, cancellationToken);
        var order = await orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        try
        {
            order.ChangeStatus(newStatus);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationException(ex.Message);
        }

        orders.Update(order);
        await uow.SaveChangesAsync(cancellationToken);

        return order;
    }

    private async Task<User> GetCallerAsync(Guid callerUserId, bool requireAdmin, CancellationToken cancellationToken)
    {
        var caller = await users.GetByIdAsync(callerUserId, cancellationToken);
        if (caller is null)
            throw new NotFoundException("Caller not found.");

        if (requireAdmin && !caller.IsAdmin)
            throw new UnauthorizedException("Administrator privileges are required.");

        return caller;
    }
}