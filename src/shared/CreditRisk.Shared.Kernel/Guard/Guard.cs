// File: src/shared/CreditRisk.Shared.Kernel/Guard/Guard.cs
namespace CreditRisk.Shared.Kernel.Guard;

/// <summary>
/// Provides guard clause methods for validating preconditions.
/// Throws <see cref="ArgumentException"/> variants for invalid inputs.
/// </summary>
public static class Guard
{
    /// <summary>Throws <see cref="ArgumentNullException"/> if value is null.</summary>
    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    /// <summary>Throws <see cref="ArgumentException"/> if string is null or whitespace.</summary>
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' must not be null or whitespace.", paramName);
        return value;
    }

    /// <summary>Throws <see cref="ArgumentException"/> if Guid is empty.</summary>
    public static Guid AgainstEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"'{paramName}' must not be an empty Guid.", paramName);
        return value;
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if value is negative.</summary>
    public static decimal AgainstNegative(decimal value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must not be negative.");
        return value;
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if value is not positive.</summary>
    public static decimal AgainstZeroOrNegative(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be greater than zero.");
        return value;
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if value exceeds maximum.</summary>
    public static decimal AgainstExceedingMaximum(decimal value, decimal maximum, string paramName)
    {
        if (value > maximum)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must not exceed {maximum}.");
        return value;
    }
}
