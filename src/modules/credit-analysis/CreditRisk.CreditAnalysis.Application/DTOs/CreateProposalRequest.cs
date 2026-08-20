// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/DTOs/CreateProposalRequest.cs
namespace CreditRisk.CreditAnalysis.Application.DTOs;

public sealed record CreateProposalRequest
{
    public required string CustomerDocument { get; init; }
    public required string CustomerDocumentType { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required decimal MonthlyIncome { get; init; }
    public required decimal RequestedLimit { get; init; }
    public required string ProposalType { get; init; }
    public required bool BureauConsentGiven { get; init; }
    public required string BureauConsentIpAddress { get; init; }
}
