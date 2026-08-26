// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Builders/CreditProposalBuilder.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.Shared.Kernel.ValueObjects;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Builders;

/// <summary>
/// Builder for CreditProposal test objects.
/// Produces a valid Draft proposal by default.
/// </summary>
internal sealed class CreditProposalBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private decimal _requestedLimit = 10_000m;
    private string _proposalType = "Individual";
    private string _createdBy = "test-operator";
    private Guid _correlationId = Guid.NewGuid();
    private bool _submitted;
    private RiskRating? _rating;
    private decimal? _approvedLimit;
    private bool _requiresManualReview;

    public CreditProposalBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public CreditProposalBuilder WithRequestedLimit(decimal limit)
    {
        _requestedLimit = limit;
        return this;
    }

    public CreditProposalBuilder WithProposalType(string type)
    {
        _proposalType = type;
        return this;
    }

    public CreditProposalBuilder WithCreatedBy(string operatorId)
    {
        _createdBy = operatorId;
        return this;
    }

    public CreditProposalBuilder WithCorrelationId(Guid correlationId)
    {
        _correlationId = correlationId;
        return this;
    }

    public CreditProposalBuilder AsSubmitted()
    {
        _submitted = true;
        return this;
    }

    public CreditProposalBuilder WithEvaluation(RiskRating rating, decimal approvedLimit, bool requiresManualReview)
    {
        _submitted = true;
        _rating = rating;
        _approvedLimit = approvedLimit;
        _requiresManualReview = requiresManualReview;
        return this;
    }

    public CreditProposal Build()
    {
        var proposal = CreditProposal.Create(
            _customerId,
            MoneyAmount.Create(_requestedLimit),
            _proposalType,
            _createdBy,
            _correlationId);

        if (_submitted && _rating.HasValue && _approvedLimit.HasValue)
        {
            proposal.Submit();
            proposal.ApplyEvaluation(_rating.Value, MoneyAmount.Create(_approvedLimit.Value), _requiresManualReview);
        }
        else if (_submitted)
        {
            proposal.Submit();
        }

        return proposal;
    }
}
