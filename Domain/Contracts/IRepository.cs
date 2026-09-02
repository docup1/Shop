namespace Domain.Contracts;

/// <summary>
/// Базовый репозиторий, манипулирующий коллекцией доменных сущностей без
/// загрузки в память целиком. Сохранение изменений выполняется вызывающим кодом
/// через SaveChanges (EF Core сам управляет транзакцией).
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}