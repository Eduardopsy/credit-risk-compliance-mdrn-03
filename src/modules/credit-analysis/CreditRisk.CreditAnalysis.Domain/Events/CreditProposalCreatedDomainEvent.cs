// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Events/CreditProposalCreatedDomainEvent.cs
using CreditRisk.Shared.Kernel.Domain;

namespace CreditRisk.CreditAnalysis.Domain.Events;

public sealed record CreditProposalCreatedDomainEvent : DomainEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal RequestedLimit { get; init; }
    public required string ProposalType { get; init; }
}
