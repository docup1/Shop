using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Tests.Repositories;

/// <summary>
/// Общая коллекция для всех тестов, работающих с PostgreSQL. Тесты внутри коллекции
/// выполняются последовательно и чистят таблицы перед собой, поэтому данные не пересекаются.
/// </summary>
[CollectionDefinition(RepositoriesCollection.Name)]
public sealed class RepositoriesCollection : ICollectionFixture<RepositoriesFixture>
{
    public const string Name = "Repositories";
}

/// <summary>
/// Жизненный цикл тестовой БД: создаёт базу <c>shop_test</c> в локальном Docker PostgreSQL
/// (см. docker-compose.yml) и накатывает миграции при первом запуске.
/// </summary>
public sealed class RepositoriesFixture : IAsyncLifetime
{
    private const string DatabaseName = "shop_test";
    private const string MasterConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

    public DbContextOptions<ApplicationDbContext> Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await EnsureDatabaseExistsAsync();

        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(new NpgsqlConnectionStringBuilder(MasterConnectionString) { Database = DatabaseName }.ConnectionString)
            .Options;

        await using var db = new ApplicationDbContext(Options);
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task EnsureDatabaseExistsAsync()
    {
        await using var connection = new NpgsqlConnection(MasterConnectionString);
        await connection.OpenAsync();

        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @db", connection);
        exists.Parameters.AddWithValue("db", DatabaseName);

        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{DatabaseName}\"", connection);
            await create.ExecuteNonQueryAsync();
        }
    }
}