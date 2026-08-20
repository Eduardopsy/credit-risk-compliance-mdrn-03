// File: src/shared/CreditRisk.Shared.Kernel/ValueObjects/MoneyAmount.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.Shared.Kernel.ValueObjects;

/// <summary>
/// Represents a monetary amount in Brazilian Reais (BRL).
/// Always non-negative. Precision: 2 decimal places.
/// </summary>
public sealed class MoneyAmount : ValueObject
{
    /// <summary>Gets the monetary amount value.</summary>
    public decimal Amount { get; }

    /// <summary>Gets the currency code. Always "BRL" for this system.</summary>
    public string Currency { get; } = "BRL";

    private MoneyAmount(decimal amount) => Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    /// <summary>Creates a new <see cref="MoneyAmount"/>. Amount must be non-negative.</summary>
    public static MoneyAmount Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("MoneyAmount.Negative", "Monetary amount must be non-negative.");
        return new MoneyAmount(amount);
    }

    /// <summary>Creates a zero monetary amount.</summary>
    public static MoneyAmount Zero() => new(0m);

    /// <summary>Adds two monetary amounts.</summary>
    public MoneyAmount Add(MoneyAmount other)
    {
        Guard.Guard.AgainstNull(other, nameof(other));
        return new MoneyAmount(Amount + other.Amount);
    }

    /// <summary>Subtracts a monetary amount. Result must be non-negative.</summary>
    public MoneyAmount Subtract(MoneyAmount other)
    {
        Guard.Guard.AgainstNull(other, nameof(other));
        if (Amount < other.Amount)
            throw new DomainException("MoneyAmount.NegativeResult", "Subtraction would result in a negative amount.");
        return new MoneyAmount(Amount - other.Amount);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Amount:F2} {Currency}";
}
