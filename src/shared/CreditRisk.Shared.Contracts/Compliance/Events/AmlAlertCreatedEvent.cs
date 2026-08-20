// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/AmlAlertCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>
/// Published when an AML alert is created for a flagged transaction.
/// Consumed by: Operations module (SignalR push to compliance-analyst group).
/// </summary>
public sealed record AmlAlertCreatedEvent
{
    public required Guid AlertId { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string AlertType { get; init; }          // "Smurfing" | "VelocityAnomaly" | "AmountAnomaly" | "PepMatch"
    public required string Severity { get; init; }           // "Low" | "Medium" | "High" | "Critical"
    public required decimal TransactionAmount { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
