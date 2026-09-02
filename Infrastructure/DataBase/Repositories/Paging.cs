using Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

/// <summary>
/// Вспомогательные методы для cursor-based пагинации (WHERE id &gt; @cursor LIMIT N
/// для ASC-сортировки, WHERE id &lt; @cursor для DESC). В отличие от OFFSET, запрос
/// идёт по индексу первичного ключа и не замедляется с ростом номера страницы.
/// </summary>
internal static class Paging
{
    /// <summary>
    /// Применяет cursor-пагинацию к запросу. Считается, что source уже отсортирован
    /// по Id (ascending или descending). Запрос выбирает pageSize + 1 записей, чтобы
    /// определить наличие следующей страницы. toDomain — проекция в доменную модель,
    /// getId — извлечение Id из материализованной сущности для следующего cursor.
    /// </summary>
    public static async Task<Page<TOut>> ToPageAsync<TEntity, TOut>(
        IQueryable<TEntity> source,
        QueryParams queryParams,
        Func<TEntity, TOut> toDomain,
        Func<TEntity, Guid> getId,
        bool descending = false,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(queryParams);

        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var cursorValue = queryParams.Cursor is { Length: > 0 } cursor ? cursor : null;

        Guid? cursorId = null;
        if (cursorValue is not null)
        {
            if (!Guid.TryParse(cursorValue, out var parsed))
                throw new ArgumentException("Cursor must be a valid Guid.", nameof(queryParams));
            cursorId = parsed;
        }

        IQueryable<TEntity> query = cursorId is null
            ? source
            : descending
                ? source.Where(e => EF.Property<Guid>(e, "Id") < cursorId.Value)
                : source.Where(e => EF.Property<Guid>(e, "Id") > cursorId.Value);

        // Выбираем на одну запись больше, чтобы понять, есть ли следующая страница.
        var entities = await query
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var hasMore = entities.Count > pageSize;
        var pageEntities = entities.Take(pageSize).ToList();
        var nextCursor = hasMore ? getId(pageEntities[^1]).ToString() : null;
        var items = pageEntities.Select(toDomain).ToList();

        return new Page<TOut>(items, nextCursor);
    }
}