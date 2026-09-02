using Domain.Contracts;
using Domain.Models;
using Infrastructure.Hash;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.DataBase;

/// <summary>
/// Гарантирует наличие администратора при старте приложения. Если пользователь
/// с логином из конфигурации уже существует — ничего не делает (не сбрасывает
/// пароль и роль существующего пользователя). В противном случае создаёт админа
/// с паролем из конфигурации.
/// </summary>
public sealed class AdminSeeder(
    IUserRepository users,
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    IOptions<AdminSeedOptions> options,
    ILogger<AdminSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seed = options.Value;
        var userName = seed.UserName?.Trim();
        if (string.IsNullOrWhiteSpace(userName))
            throw new InvalidOperationException($"'{AdminSeedOptions.SectionName}:UserName' must be configured.");

        if (string.IsNullOrWhiteSpace(seed.Password))
            throw new InvalidOperationException($"'{AdminSeedOptions.SectionName}:Password' must be configured.");

        if (await users.ExistsByUserNameAsync(userName, cancellationToken))
        {
            logger.LogInformation("Admin user '{UserName}' already exists; skipping seed.", userName);
            return;
        }

        var admin = User.Create(userName, passwordHasher.Hash(seed.Password), isAdmin: true);
        await users.AddAsync(admin, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded admin user '{UserName}'.", userName);
    }
}
