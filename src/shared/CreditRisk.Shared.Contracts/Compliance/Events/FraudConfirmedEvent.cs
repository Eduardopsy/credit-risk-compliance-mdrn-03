// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/FraudConfirmedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>Published when a compliance analyst confirms a transaction as fraud.</summary>
public sealed record FraudConfirmedEvent
{
    public required Guid AlertId { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string ConfirmedBy { get; init; }        // Analyst user ID
    public required string FraudType { get; init; }
    public required DateTimeOffset ConfirmedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
