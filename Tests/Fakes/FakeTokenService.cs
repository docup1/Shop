using Domain.Contracts;

namespace Tests.Fakes;

/// <summary>Детерминированный фейк access-токена: строковый формат userId:sessionId.</summary>
internal sealed class FakeTokenService : ITokenService
{
    public string GenerateAccessToken(Guid userId, string userName, Guid sessionId)
        => $"{userId}:{sessionId}";
}