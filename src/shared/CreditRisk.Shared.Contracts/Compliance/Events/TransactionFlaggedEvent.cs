// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/TransactionFlaggedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>Published when a transaction is flagged by the AML rules engine.</summary>
public sealed record TransactionFlaggedEvent
{
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string FlagReason { get; init; }
    public required string[] TriggeredRules { get; init; }
    public required DateTimeOffset FlaggedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
