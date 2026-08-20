// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Redis/RedisTokenRevocationStore.cs
using CreditRisk.IAM.Application.Ports;
using StackExchange.Redis;

namespace CreditRisk.IAM.Infrastructure.Redis;

public sealed class RedisTokenRevocationStore(IConnectionMultiplexer redis) : ITokenRevocationStore
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task RevokeAsync(string jti, TimeSpan expiresIn, CancellationToken cancellationToken = default)
    {
        await _db.StringSetAsync($"revoked:{jti}", "1", expiresIn).ConfigureAwait(false);
    }

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        return await _db.KeyExistsAsync($"revoked:{jti}").ConfigureAwait(false);
    }
}
