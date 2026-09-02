using Domain.Contracts;

namespace Tests.Fakes;

/// <summary>Обратимый фейк: Hash добавляет фиксированный префикс, Verify сравнивает.</summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    private const string Prefix = "hash:";

    public string Hash(string password) => Prefix + password;

    public bool Verify(string password, string hash)
        => hash == Prefix + password;
}