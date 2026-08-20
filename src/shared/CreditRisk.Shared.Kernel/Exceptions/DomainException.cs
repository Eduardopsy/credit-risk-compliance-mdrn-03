// File: src/shared/CreditRisk.Shared.Kernel/Exceptions/DomainException.cs
namespace CreditRisk.Shared.Kernel.Exceptions;

/// <summary>
/// Base exception for all domain-level violations.
/// Thrown only for programming errors (invariant violations), not expected business failures.
/// Expected business failures must use Result pattern instead.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
