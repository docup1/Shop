using Domain.Contracts;

namespace Infrastructure.Hash;

/// <summary>
/// Хэширование паролей через BCrypt. Work factor задаётся при создании и
/// вшивается в сам хэш, поэтому значение можно менять со временем без миграций.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public PasswordHasher(int workFactor = 11)
    {
        _workFactor = workFactor;
    }

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    public bool Verify(string password, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}