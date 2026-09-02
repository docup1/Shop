using Infrastructure.DataBase.Entities;

namespace Infrastructure.DataBase.Mapping;

/// <summary>
/// Преобразование между доменными моделями (Domain.Models) и EF-сущностями.
/// Чистое проекционное отображение: создание новых сущностей идёт через фабрики
/// доменных моделей, чтение из БД восстанавливается через Restore. Маппер лишь
/// копирует значения и защищает от некорректных входных данных.
/// </summary>
public static class EntityMapper
{
    public static Entities.User ToEntity(Domain.Models.User domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new Entities.User
        {
            Id = domain.Id,
            UserName = domain.UserName,
            PasswordHash = domain.PasswordHash,
            IsAdmin = domain.IsAdmin
        };
    }

    public static Domain.Models.User ToDomain(Entities.User entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return Domain.Models.User.Restore(
            entity.Id,
            NonEmpty(entity.UserName, nameof(entity.UserName)),
            NonEmpty(entity.PasswordHash, nameof(entity.PasswordHash)),
            entity.IsAdmin);
    }

    public static Entities.Order ToEntity(Domain.Models.Order domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new Entities.Order
        {
            Id = domain.Id,
            UserId = domain.UserId,
            SenderCity = domain.SenderCity,
            RecipientCity = domain.RecipientCity,
            SenderAddress = domain.SenderAddress,
            RecipientAddress = domain.RecipientAddress,
            Weight = domain.Weight,
            Status = domain.Status,
            CreatedAt = domain.CreatedAt
        };
    }

    public static Domain.Models.Order ToDomain(Entities.Order entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return Domain.Models.Order.Restore(
            entity.Id,
            entity.UserId,
            NonEmpty(entity.SenderCity, nameof(entity.SenderCity)),
            NonEmpty(entity.RecipientCity, nameof(entity.RecipientCity)),
            NonEmpty(entity.SenderAddress, nameof(entity.SenderAddress)),
            NonEmpty(entity.RecipientAddress, nameof(entity.RecipientAddress)),
            entity.Weight,
            entity.Status,
            entity.CreatedAt);
    }

    public static Entities.Session ToEntity(Domain.Models.Session domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new Entities.Session
        {
            Id = domain.Id,
            UserId = domain.UserId,
            ExpiresAt = domain.ExpiresAt,
            CreatedAt = domain.CreatedAt
        };
    }

    public static Domain.Models.Session ToDomain(Entities.Session entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return Domain.Models.Session.Restore(
            entity.Id,
            entity.UserId,
            entity.ExpiresAt,
            entity.CreatedAt);
    }

    private static string NonEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' must not be empty.");

        return value.Trim();
    }
}
