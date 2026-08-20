// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/ValueObjects/RiskScore.cs
using CreditRisk.Shared.Kernel.Domain;

namespace CreditRisk.CreditAnalysis.Domain.ValueObjects;

public sealed class RiskScore : ValueObject
{
    public int Value { get; }

    private RiskScore(int value) => Value = value;

    public static RiskScore Create(int value)
    {
        if (value < 0 || value > 1000)
            throw new ArgumentOutOfRangeException(nameof(value), "Risk score must be between 0 and 1000.");
        return new RiskScore(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
