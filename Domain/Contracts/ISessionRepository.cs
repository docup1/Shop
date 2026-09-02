using Domain.Models;

namespace Domain.Contracts;

/// <summary>
/// Репозиторий сессий: CRUD (через <see cref="IRepository{TEntity}"/>) плюс
/// запросы по активным и истёкшим сессиям (refresh-токенам).
/// </summary>
public interface ISessionRepository : IRepository<Session>
{
    Task<Session?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Page<Session>> GetActiveByUserIdAsync(Guid userId, QueryParams queryParams, CancellationToken cancellationToken = default);

    Task<Page<Session>> GetExpiredAsync(QueryParams queryParams, CancellationToken cancellationToken = default);
}