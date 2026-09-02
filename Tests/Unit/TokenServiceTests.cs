using System.IdentityModel.Tokens.Jwt;
using Infrastructure.JWT;
using Microsoft.Extensions.Options;
using Tests.Fakes;

namespace Tests.Unit;

public class TokenServiceTests
{
    private const string SigningKey = "test-signing-key-0123456789abcdef0123456789abcdef";
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private static TokenService CreateService(JwtOptions? options = null)
    {
        options ??= new JwtOptions
        {
            Issuer = "shop.tests",
            Audience = "shop.tests",
            SigningKey = SigningKey,
            AccessTokenLifetimeMinutes = 15,
            SessionLifetimeDays = 30
        };

        return new TokenService(Options.Create(options), new FakeTimeProvider(Now));
    }

    private static JwtSecurityToken Decode(string token)
        => new JwtSecurityTokenHandler().ReadJwtToken(token);

    private static string Claim(JwtSecurityToken token, string type)
        => Assert.Single(token.Claims, c => c.Type == type).Value;

    [Fact]
    public void GenerateAccessToken_ContainsExpectedClaims()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var token = Decode(CreateService().GenerateAccessToken(userId, "alice", sessionId));

        Assert.Equal("shop.tests", token.Issuer);
        Assert.Contains("shop.tests", token.Audiences);
        Assert.Equal(userId.ToString(), Claim(token, JwtRegisteredClaimNames.Sub));
        Assert.Equal("alice", Claim(token, JwtRegisteredClaimNames.Name));
        Assert.Equal(sessionId.ToString(), Claim(token, JwtRegisteredClaimNames.Sid));
        Assert.True(Guid.TryParse(Claim(token, JwtRegisteredClaimNames.Jti), out _));
    }

    [Fact]
    public void GenerateAccessToken_SetsExpiryFromTimeProvider()
    {
        var token = Decode(CreateService().GenerateAccessToken(Guid.NewGuid(), "alice", Guid.NewGuid()));

        var exp = long.Parse(Claim(token, JwtRegisteredClaimNames.Exp));
        var expected = new DateTimeOffset(Now.UtcDateTime.AddMinutes(15)).ToUnixTimeSeconds();

        Assert.Equal(expected, exp);
    }

    [Fact]
    public void GenerateAccessToken_DifferentSession_YieldsDifferentToken()
    {
        var service = CreateService();
        var first = service.GenerateAccessToken(Guid.NewGuid(), "alice", Guid.NewGuid());
        var second = service.GenerateAccessToken(Guid.NewGuid(), "alice", Guid.NewGuid());

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void GenerateAccessToken_MissingSigningKey_Throws()
    {
        var service = CreateService(new JwtOptions { Issuer = "x", Audience = "x" });

        Assert.Throws<ArgumentException>(() => service.GenerateAccessToken(Guid.NewGuid(), "alice", Guid.NewGuid()));
    }

    [Fact]
    public void GenerateAccessToken_NullUserName_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.GenerateAccessToken(Guid.NewGuid(), null!, Guid.NewGuid()));
    }
}