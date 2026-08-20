// File: src/modules/iam/CreditRisk.IAM.Domain/Entities/RefreshToken.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.IAM.Domain.Entities;

/// <summary>Represents a refresh token for JWT authentication.</summary>
public sealed class RefreshToken : Entity
{
    public Guid UserId { get; private init; }
    public string Token { get; private init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private init; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() : base() { }

    public static RefreshToken Create(Guid userId, string token, DateTimeOffset expiresAt)
    {
        Guard.AgainstEmpty(userId, nameof(userId));
        Guard.AgainstNullOrWhiteSpace(token, nameof(token));

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
