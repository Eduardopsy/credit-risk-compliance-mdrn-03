// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/DTOs/ProposalAcceptedResponse.cs
namespace CreditRisk.CreditAnalysis.Application.DTOs;

public sealed record ProposalAcceptedResponse
{
    public required Guid ProposalId { get; init; }
}
