// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/AmlAlertCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>
/// Published when an AML alert is created for a flagged transaction.
/// One event per alert (multiple alerts possible per transaction).
/// </summary>
public sealed record AmlAlertCreatedEvent
{
    /// <summary>Unique alert identifier.</summary>
    public required Guid AlertId { get; init; }

    /// <summary>Transaction that triggered the alert.</summary>
    public required Guid TransactionId { get; init; }

    /// <summary>Customer involved in the flagged transaction.</summary>
    public required Guid CustomerId { get; init; }

    /// <summary>Type of AML rule that triggered (Smurfing, PepMatch, etc.).</summary>
    public required string AlertType { get; init; }

    /// <summary>Severity level (Low, Medium, High, Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Amount of the flagged transaction.</summary>
    public required decimal TransactionAmount { get; init; }

    /// <summary>When the alert was created.</summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>Correlation ID for distributed tracing.</summary>
    public required Guid CorrelationId { get; init; }
}
