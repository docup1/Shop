using Infrastructure.Hash;

namespace Tests.Unit;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsNonEmptyHashDifferentFromPassword()
    {
        var hash = _hasher.Hash("secret");

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual("secret", hash);
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashes()
    {
        var first = _hasher.Hash("secret");
        var second = _hasher.Hash("secret");

        Assert.NotEqual(first, second);
        Assert.True(_hasher.Verify("secret", first));
        Assert.True(_hasher.Verify("secret", second));
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("secret");

        Assert.True(_hasher.Verify("secret", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("secret");

        Assert.False(_hasher.Verify("wrong", hash));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Hash_InvalidPassword_Throws(string? password)
        => Assert.ThrowsAny<ArgumentException>(() => _hasher.Hash(password!));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_InvalidPassword_Throws(string? password)
        => Assert.ThrowsAny<ArgumentException>(() => _hasher.Verify(password!, "hash"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_InvalidHash_Throws(string? hash)
        => Assert.ThrowsAny<ArgumentException>(() => _hasher.Verify("password", hash!));
}