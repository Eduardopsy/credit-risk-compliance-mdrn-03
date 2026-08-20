// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditLimitApprovedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>Published when a credit limit is auto-approved (Rating A or B, limit below threshold).</summary>
public sealed record CreditLimitApprovedEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal ApprovedLimit { get; init; }
    public required string RiskRating { get; init; }
    public required DateTimeOffset ApprovedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
