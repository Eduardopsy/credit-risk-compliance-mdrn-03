// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/TransactionFlaggedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>
/// Published when a transaction is flagged by any AML rule.
/// Aggregates all rules triggered for the transaction.
/// </summary>
public sealed record TransactionFlaggedEvent
{
    /// <summary>The flagged transaction identifier.</summary>
    public required Guid TransactionId { get; init; }

    /// <summary>Customer involved in the flagged transaction.</summary>
    public required Guid CustomerId { get; init; }

    /// <summary>Summary reason for flagging (comma-separated list of triggered rules).</summary>
    public required string FlagReason { get; init; }

    /// <summary>Array of all rules that triggered.</summary>
    public required string[] TriggeredRules { get; init; }

    /// <summary>When the transaction was flagged.</summary>
    public required DateTimeOffset FlaggedAt { get; init; }

    /// <summary>Correlation ID for distributed tracing.</summary>
    public required Guid CorrelationId { get; init; }
}
