// File: src/modules/iam/CreditRisk.IAM.Domain/ValueObjects/Email.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.IAM.Domain.ValueObjects;

/// <summary>Represents a validated user email address.</summary>
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        string trimmed = value.Trim().ToLowerInvariant();

        if (!trimmed.Contains('@', StringComparison.Ordinal) || trimmed.Length < 5)
            throw new DomainException("Email.Invalid", "Invalid email address format.");

        return new Email(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
