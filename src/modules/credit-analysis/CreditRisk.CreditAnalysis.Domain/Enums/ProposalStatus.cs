// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Enums/ProposalStatus.cs
namespace CreditRisk.CreditAnalysis.Domain.Enums;

/// <summary>Lifecycle states of a credit proposal.</summary>
public enum ProposalStatus
{
    Draft = 0,
    PendingEvaluation = 1,
    PendingReview = 2,
    Approved = 3,
    Rejected = 4
}
