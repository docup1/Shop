using Domain.Contracts;
using Domain.Models;
using Domain.Types;

namespace Tests.Fakes;

/// <summary>
/// In-memory реализация <see cref="IOrderRepository"/> для unit-тестов Application.
/// </summary>
internal sealed class InMemoryOrderRepository : IOrderRepository
{
    public List<Order> Items { get; } = [];

    public Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(o => o.Id == id));

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(o => o.Id == id));

    public void Update(Order entity)
    {
        var index = Items.FindIndex(o => o.Id == entity.Id);
        if (index >= 0)
            Items[index] = entity;
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.RemoveAll(o => o.Id == id) > 0);

    public Task<Page<Order>> GetByUserIdAsync(Guid userId, QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var items = Items.Where(o => o.UserId == userId).OrderByDescending(o => o.Id).ToArray();
        return Task.FromResult(new Page<Order>(items, null));
    }

    public Task<Page<Order>> GetAllAsync(QueryParams queryParams, Status? status = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Order> query = Items;

        if (status is not null)
            query = query.Where(o => o.Status == status.Value);

        var items = query.OrderByDescending(o => o.Id).ToArray();
        return Task.FromResult(new Page<Order>(items, null));
    }
}