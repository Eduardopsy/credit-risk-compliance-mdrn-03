// File: src/modules/iam/CreditRisk.IAM.Domain/ValueObjects/HashedPassword.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.IAM.Domain.ValueObjects;

/// <summary>Represents a securely hashed password.</summary>
public sealed class HashedPassword : ValueObject
{
    public string Value { get; }

    private HashedPassword(string value) => Value = value;

    public static HashedPassword Create(string hash)
    {
        Guard.AgainstNullOrWhiteSpace(hash, nameof(hash));
        return new HashedPassword(hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
