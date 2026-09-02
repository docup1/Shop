using Domain.Contracts;
using Domain.Models;

namespace Tests.Fakes;

/// <summary>
/// In-memory реализация <see cref="IUserRepository"/> для unit-тестов Application.
/// <see cref="Items"/> открыт, чтобы тесты могли напрямую сидировать пользователей.
/// </summary>
internal sealed class InMemoryUserRepository : IUserRepository
{
    public List<User> Items { get; } = [];

    public Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(u => u.Id == id));

    public void Update(User entity)
    {
        var index = Items.FindIndex(u => u.Id == entity.Id);
        if (index >= 0)
            Items[index] = entity;
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.RemoveAll(u => u.Id == id) > 0);

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(u => u.UserName == userName));

    public Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(u => u.UserName == userName));

    public Task<Page<User>> GetAdminsAsync(QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var admins = Items.Where(u => u.IsAdmin).OrderBy(u => u.Id).ToArray();
        return Task.FromResult(new Page<User>(admins, null));
    }

    public Task<Page<User>> GetAllAsync(QueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var all = Items.OrderBy(u => u.Id).ToArray();
        return Task.FromResult(new Page<User>(all, null));
    }
}