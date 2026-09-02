using Domain.Contracts;

namespace Tests.Fakes;

/// <summary>
/// In-memory реализация <see cref="IUnitOfWork"/>: считает вызовы SaveChanges,
/// чтобы проверять, что сервисы действительно коммитят изменения.
/// </summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(SaveCount);
    }
}