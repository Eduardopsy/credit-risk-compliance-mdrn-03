// File: src/shared/CreditRisk.Shared.Kernel/ValueObjects/Cpf.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.Shared.Kernel.ValueObjects;

/// <summary>
/// Represents a Brazilian CPF (Cadastro de Pessoas Físicas) document number.
/// Validates using the official Receita Federal check digit algorithm.
/// Stored as 11 digits without formatting (no dots or dashes).
/// </summary>
public sealed class Cpf : ValueObject
{
    /// <summary>Gets the CPF value as 11 unformatted digits.</summary>
    public string Value { get; }

    private Cpf(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="Cpf"/> instance after validating the check digits.
    /// </summary>
    /// <param name="value">CPF string — may include formatting (dots and dashes) or be 11 raw digits.</param>
    /// <returns>A valid <see cref="Cpf"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the CPF fails check digit validation.</exception>
    public static Cpf Create(string value)
    {
        Guard.Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        string digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            throw new DomainException("CPF.InvalidLength", $"CPF must have exactly 11 digits. Got: {digits.Length}.");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CPF.AllSameDigits", "CPF with all identical digits is invalid.");

        if (!ValidateCheckDigits(digits))
            throw new DomainException("CPF.InvalidCheckDigit", "CPF check digits are invalid.");

        return new Cpf(digits);
    }

    /// <summary>Returns the CPF formatted as XXX.XXX.XXX-XX.</summary>
    public string ToFormattedString() => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..11]}";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    private static bool ValidateCheckDigits(string digits)
    {
        // First check digit
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (digits[i] - '0') * (10 - i);

        int remainder = sum % 11;
        int firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        if ((digits[9] - '0') != firstCheckDigit)
            return false;

        // Second check digit
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (digits[i] - '0') * (11 - i);

        remainder = sum % 11;
        int secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        return (digits[10] - '0') == secondCheckDigit;
    }
}
