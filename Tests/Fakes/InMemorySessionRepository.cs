using Domain.Contracts;
using Domain.Models;

namespace Tests.Fakes;

/// <summary>
/// In-memory реализация <see cref="ISessionRepository"/>. Проверки «активна ли
/// сессия» используют <see cref="TimeProvider"/>, чтобы тесты могли сдвигать время.
/// </summary>
internal sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly TimeProvider _timeProvider;

    public InMemorySessionRepository(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public List<Session> Items { get; } = [];

    public Task<Session> AddAsync(Session entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(s => s.Id == id));

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(s => s.Id == id));

    public void Update(Session entity)
    {
        throw new NotSupportedException("Sessions are immutable and never updated.");
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.RemoveAll(s => s.Id == id) > 0);

    public Task<Session?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        return Task.FromResult(Items.FirstOrDefault(s => s.Id == id && s.ExpiresAt > now));
    }

    public Task<Page<Session>> GetActiveByUserIdAsync(Guid userId, QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var items = Items
            .Where(s => s.UserId == userId && s.ExpiresAt > now)
            .OrderByDescending(s => s.Id)
            .ToArray();

        return Task.FromResult(new Page<Session>(items, null));
    }

    public Task<Page<Session>> GetExpiredAsync(QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var items = Items.Where(s => s.ExpiresAt <= now).OrderByDescending(s => s.Id).ToArray();
        return Task.FromResult(new Page<Session>(items, null));
    }
}