// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Commands/TransactionReceivedCommand.cs
namespace CreditRisk.Shared.Contracts.Compliance.Commands;

/// <summary>
/// Command: A transaction has been received and requires compliance screening.
/// Triggers AML/CFT rules evaluation, PEP screening, and alert creation.
/// </summary>
public sealed record TransactionReceivedCommand
{
    /// <summary>Unique transaction identifier.</summary>
    public required Guid TransactionId { get; init; }

    /// <summary>Customer performing the transaction.</summary>
    public required Guid CustomerId { get; init; }

    /// <summary>Customer's document (CPF/CNPJ) for PEP screening.</summary>
    public required string CustomerDocument { get; init; }

    /// <summary>Transaction amount in currency units.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Type of transaction (Online, ATM, Branch, etc.).</summary>
    public required string TransactionType { get; init; }

    /// <summary>Channel used for transaction (Web, Mobile, ATM, etc.).</summary>
    public required string Channel { get; init; }

    /// <summary>When the transaction occurred.</summary>
    public required DateTimeOffset TransactionDate { get; init; }

    /// <summary>Correlation ID for distributed tracing.</summary>
    public required Guid CorrelationId { get; init; }
}
