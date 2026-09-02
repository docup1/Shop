namespace Domain.Contracts;

/// <summary>Хэширование и проверка паролей (BCrypt).</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}