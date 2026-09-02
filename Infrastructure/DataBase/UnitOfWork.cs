using Domain.Contracts;

namespace Infrastructure.DataBase;

/// <summary>
/// Реализация <see cref="IUnitOfWork"/> поверх EF Core: единственный путь коммита
/// для Application-слоя. Транзакцией управляет сам DbContext (SaveChanges атомарен).
/// </summary>
public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}