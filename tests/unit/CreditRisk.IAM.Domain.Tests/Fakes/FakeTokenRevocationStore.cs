// File: tests/unit/CreditRisk.IAM.Domain.Tests/Fakes/FakeTokenRevocationStore.cs
using CreditRisk.IAM.Application.Ports;

namespace CreditRisk.IAM.Domain.Tests.Fakes;

/// <summary>
/// In-memory fake for ITokenRevocationStore.
/// Supports configuring specific JTIs as revoked for testing revocation behavior.
/// </summary>
internal sealed class FakeTokenRevocationStore : ITokenRevocationStore
{
    private readonly HashSet<string> _revokedJtis = [];

    public Task RevokeAsync(string jti, TimeSpan expiresIn, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _revokedJtis.Add(jti);
        return Task.CompletedTask;
    }

    public Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_revokedJtis.Contains(jti));
    }

    // Test helpers
    public void PreloadRevoked(string jti) => _revokedJtis.Add(jti);

    public bool Contains(string jti) => _revokedJtis.Contains(jti);

    public void Clear() => _revokedJtis.Clear();
}
