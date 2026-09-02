using Domain.Models;
using Domain.Types;

namespace Domain.Contracts;

/// <summary>
/// Репозиторий заказов: CRUD (через <see cref="IRepository{TEntity}"/>) плюс
/// выборки для каталога пользователя и админ-панели.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Заказы конкретного пользователя, новые сверху (сортировка по Id DESC).</summary>
    Task<Page<Order>> GetByUserIdAsync(Guid userId, QueryParams queryParams, CancellationToken cancellationToken = default);

    /// <summary>Все заказы с опциональным фильтром по статусу, новые сверху.</summary>
    Task<Page<Order>> GetAllAsync(QueryParams queryParams, Status? status = null, CancellationToken cancellationToken = default);
}