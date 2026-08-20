// File: src/shared/CreditRisk.Shared.Kernel/ValueObjects/Cnpj.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.Shared.Kernel.ValueObjects;

/// <summary>
/// Represents a Brazilian CNPJ (Cadastro Nacional da Pessoa Jurídica) document number.
/// Validates using the official Receita Federal check digit algorithm.
/// Stored as 14 digits without formatting.
/// </summary>
public sealed class Cnpj : ValueObject
{
    /// <summary>Gets the CNPJ value as 14 unformatted digits.</summary>
    public string Value { get; }

    private Cnpj(string value) => Value = value;

    /// <summary>Creates a new <see cref="Cnpj"/> after validating check digits.</summary>
    public static Cnpj Create(string value)
    {
        Guard.Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        string digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 14)
            throw new DomainException("CNPJ.InvalidLength", $"CNPJ must have exactly 14 digits. Got: {digits.Length}.");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CNPJ.AllSameDigits", "CNPJ with all identical digits is invalid.");

        if (!ValidateCheckDigits(digits))
            throw new DomainException("CNPJ.InvalidCheckDigit", "CNPJ check digits are invalid.");

        return new Cnpj(digits);
    }

    /// <summary>Returns the CNPJ formatted as XX.XXX.XXX/XXXX-XX.</summary>
    public string ToFormattedString()
        => $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..14]}";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    private static bool ValidateCheckDigits(string digits)
    {
        int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (digits[i] - '0') * weights1[i];

        int remainder = sum % 11;
        int firstCheck = remainder < 2 ? 0 : 11 - remainder;

        if ((digits[12] - '0') != firstCheck) return false;

        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += (digits[i] - '0') * weights2[i];

        remainder = sum % 11;
        int secondCheck = remainder < 2 ? 0 : 11 - remainder;

        return (digits[13] - '0') == secondCheck;
    }
}
