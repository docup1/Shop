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

    public static User Create(string userName, string passwordHash, bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("UserName cannot be empty");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be empty");

        return new User(userName, passwordHash, isAdmin);
    }
}
