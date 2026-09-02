namespace Domain.Contracts;

/// <summary>
/// Граница сохранения: коммит изменений, накопленных репозиториями за текущую
/// операцию. Репозитории не вызывают SaveChanges — это делает Application после
/// мутаций. EF Core сам управляет транзакцией.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}