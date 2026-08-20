// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Entities/CreditProposal.cs
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Domain.Events;
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.Guard;
using CreditRisk.Shared.Kernel.ValueObjects;

namespace CreditRisk.CreditAnalysis.Domain.Entities;

/// <summary>
/// Aggregate root representing a credit proposal submitted by a desk operator.
/// Manages the lifecycle from Draft through Approved/Rejected.
/// </summary>
public sealed class CreditProposal : AggregateRoot
{
    public Guid CustomerId { get; private init; }
    public MoneyAmount RequestedLimit { get; private init; } = MoneyAmount.Zero();
    public MoneyAmount? ApprovedLimit { get; private set; }
    public ProposalStatus Status { get; private set; }
    public RiskRating? Rating { get; private set; }
    public bool RequiresManualReview { get; private set; }
    public string CreatedBy { get; private init; } = string.Empty;
    public string ProposalType { get; private init; } = string.Empty;

    private CreditProposal() : base() { }

    /// <summary>Creates a new credit proposal in Draft status.</summary>
    public static CreditProposal Create(
        Guid customerId,
        MoneyAmount requestedLimit,
        string proposalType,
        string createdBy,
        Guid correlationId)
    {
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstNull(requestedLimit, nameof(requestedLimit));
        Guard.AgainstNullOrWhiteSpace(proposalType, nameof(proposalType));
        Guard.AgainstNullOrWhiteSpace(createdBy, nameof(createdBy));

        if (requestedLimit.Amount <= 0)
            throw new DomainException("Proposal.InvalidLimit", "Requested limit must be greater than zero.");

        if (requestedLimit.Amount > 500_000m)
            throw new DomainException("Proposal.ExceedsMaxLimit", "Requested limit cannot exceed R$ 500,000.");

        var proposal = new CreditProposal
        {
            CustomerId = customerId,
            RequestedLimit = requestedLimit,
            ProposalType = proposalType,
            CreatedBy = createdBy,
            Status = ProposalStatus.Draft
        };

        proposal.RaiseDomainEvent(new CreditProposalCreatedDomainEvent
        {
            ProposalId = proposal.Id,
            CustomerId = customerId,
            RequestedLimit = requestedLimit.Amount,
            ProposalType = proposalType,
            CorrelationId = correlationId
        });

        return proposal;
    }

    /// <summary>Transitions the proposal from Draft to PendingEvaluation.</summary>
    public void Submit()
    {
        if (Status != ProposalStatus.Draft)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot submit a proposal in status {Status}. Only Draft proposals can be submitted.");

        Status = ProposalStatus.PendingEvaluation;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Applies the result of the scoring engine evaluation.</summary>
    public void ApplyEvaluation(RiskRating rating, MoneyAmount approvedLimit, bool requiresManualReview)
    {
        if (Status != ProposalStatus.PendingEvaluation)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot evaluate a proposal in status {Status}.");

        Rating = rating;
        ApprovedLimit = approvedLimit;
        RequiresManualReview = requiresManualReview;
        Status = requiresManualReview ? ProposalStatus.PendingReview : ProposalStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Manually approves a proposal that required review.</summary>
    public void ManuallyApprove(string approvedBy)
    {
        if (Status != ProposalStatus.PendingReview)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot manually approve a proposal in status {Status}.");

        Status = ProposalStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Rejects the proposal.</summary>
    public void Reject(string rejectedBy, string reason)
    {
        if (Status is ProposalStatus.Approved or ProposalStatus.Rejected)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot reject a proposal in status {Status}.");

        Status = ProposalStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
