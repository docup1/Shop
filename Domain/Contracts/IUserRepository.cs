using Domain.Models;

namespace Domain.Contracts;

/// <summary>
/// Репозиторий пользователей: CRUD (через <see cref="IRepository{TEntity}"/>) плюс
/// запросы, специфичные для User.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<Page<User>> GetAdminsAsync(QueryParams queryParams, CancellationToken cancellationToken = default);

    Task<Page<User>> GetAllAsync(QueryParams queryParams, CancellationToken cancellationToken = default);
}