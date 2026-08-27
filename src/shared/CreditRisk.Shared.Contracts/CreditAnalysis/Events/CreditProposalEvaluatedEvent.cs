// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalEvaluatedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>
/// Published after credit proposal evaluation (scoring completed).
/// Contains risk assessment result and approved limit.
/// </summary>
public sealed record CreditProposalEvaluatedEvent
{
    /// <summary>The evaluated proposal identifier.</summary>
    public required Guid ProposalId { get; init; }

    /// <summary>Customer associated with the proposal.</summary>
    public required Guid CustomerId { get; init; }

    /// <summary>Risk rating (A, B, C, D, E) from scoring engine.</summary>
    public required string RiskRating { get; init; }

    /// <summary>Approved credit limit amount.</summary>
    public required decimal ApprovedLimit { get; init; }

    /// <summary>When the evaluation was completed.</summary>
    public required DateTimeOffset EvaluatedAt { get; init; }

    /// <summary>Evaluator identifier (usually "AUTO" for scoring engine).</summary>
    public required string EvaluatedBy { get; init; }

    /// <summary>Whether manual review is required despite automation.</summary>
    public required bool RequiresManualReview { get; init; }

    /// <summary>Correlation ID for distributed tracing.</summary>
    public required Guid CorrelationId { get; init; }
}
