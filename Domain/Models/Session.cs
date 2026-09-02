namespace Domain.Models;

public class Session
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public DateTime ExpiresAt { get; private init; }
    public DateTime CreatedAt { get; private init; }

    private Session(Guid userId, DateTime expiresAt)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    private Session(Guid id, Guid userId, DateTime expiresAt, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public static Session Create(Guid userId, TimeSpan lifetime)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");
        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentException("Lifetime must be positive");

        return new Session(userId, DateTime.UtcNow + lifetime);
    }

    /// <summary>Восстановление полностью материализованной сущности (для чтения из БД).</summary>
    public static Session Restore(Guid id, Guid userId, DateTime expiresAt, DateTime createdAt)
        => new(id, userId, expiresAt, createdAt);

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
