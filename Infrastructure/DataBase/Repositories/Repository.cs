using Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

/// <summary>
/// Базовый generic-репозиторий. Отображает доменные модели (TDomain) на EF-сущности
/// (TEntity) через переданные функции маппинга. Не загружает данные в память целиком.
/// Сохранение выполняется вызывающим кодом через SaveChanges (EF Core сам управляет
/// транзакцией).
/// </summary>
public abstract class Repository<TDomain, TEntity>(
    DbSet<TEntity> set,
    Func<TDomain, TEntity> toEntity,
    Func<TEntity, TDomain> toDomain)
    : IRepository<TDomain>
    where TDomain : class
    where TEntity : class
{
    public async Task<TDomain> AddAsync(TDomain entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entityEntity = toEntity(entity);
        await set.AddAsync(entityEntity, cancellationToken);
        return toDomain(entityEntity);
    }

    /// <summary>
    /// Чтение без трекинга: вернувшаяся сущность не конфликтует с последующим
    /// repo.Update(toEntity(domain)) в том же контексте (иначе EF падал бы с
    /// "entity already being tracked" при identity-разрешении).
    /// </summary>
    public async Task<TDomain?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await set
            .AsNoTracking()
            .SingleOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
        return entity is null ? null : toDomain(entity);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await set.AnyAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);

    public void Update(TDomain entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        set.Update(toEntity(entity));
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await set
            .Where(e => EF.Property<Guid>(e, "Id") == id)
            .ExecuteDeleteAsync(cancellationToken);
        return deleted > 0;
    }
}