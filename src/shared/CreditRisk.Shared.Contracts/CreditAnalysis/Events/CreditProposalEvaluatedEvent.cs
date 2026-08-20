// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalEvaluatedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>
/// Published when a credit proposal has been evaluated and a risk rating assigned.
/// Consumed by: Compliance module (AML cross-check), Operations module (dashboard update).
/// </summary>
public sealed record CreditProposalEvaluatedEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string RiskRating { get; init; }         // "A" | "B" | "C" | "D" | "E"
    public required decimal ApprovedLimit { get; init; }
    public required DateTimeOffset EvaluatedAt { get; init; }
    public required string EvaluatedBy { get; init; }        // "AUTO" or operator ID
    public required bool RequiresManualReview { get; init; }
    public required Guid CorrelationId { get; init; }
}
