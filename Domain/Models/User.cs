namespace Domain.Models;

public class User
{
    public Guid Id { get; private init; }
    public string UserName { get; private init; } = string.Empty;
    public string PasswordHash { get; private init; } = string.Empty;
    public bool IsAdmin { get; private init; }

    private User(string userName, string passwordHash, bool isAdmin)
    {
        Id = Guid.CreateVersion7();
        UserName = userName;
        PasswordHash = passwordHash;
        IsAdmin = isAdmin;
    }

    private User(Guid id, string userName, string passwordHash, bool isAdmin)
    {
        Id = id;
        UserName = userName;
        PasswordHash = passwordHash;
        IsAdmin = isAdmin;
    }

    public static User Create(string userName, string passwordHash, bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("UserName cannot be empty");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be empty");

        return new User(userName, passwordHash, isAdmin);
    }

    /// <summary>Восстановление полностью материализованной сущности (для чтения из БД).</summary>
    public static User Restore(Guid id, string userName, string passwordHash, bool isAdmin)
        => new(id, userName, passwordHash, isAdmin);

    /// <summary>Возвращает копию с новой ролью (иммутабельный стиль).</summary>
    public User SetAdmin(bool isAdmin) => new(Id, UserName, PasswordHash, isAdmin);
}
