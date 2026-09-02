using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Tests.Repositories;

/// <summary>
/// База для репозиторных тестов: создаёт свежий ApplicationDbContext из фикстуры
/// и чистит таблицы перед каждым тестом.
/// </summary>
[Collection(RepositoriesCollection.Name)]
public abstract class RepositoryTestBase(RepositoriesFixture fixture)
{
    protected RepositoriesFixture Fixture { get; } = fixture;

    protected ApplicationDbContext CreateContext() => new(Fixture.Options);

    protected async Task ClearTablesAsync()
    {
        await using var db = CreateContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users, sessions, orders CASCADE");
    }
}