// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Enums/RiskRating.cs
namespace CreditRisk.CreditAnalysis.Domain.Enums;

/// <summary>
/// Credit risk rating from A (lowest risk) to E (highest risk).
/// Follows BCB Resolução 4.557 risk classification framework.
/// </summary>
public enum RiskRating
{
    A = 1,   // Score >= 750: Excellent — auto-approve up to R$ 50,000
    B = 2,   // Score 650-749: Good — auto-approve up to R$ 20,000
    C = 3,   // Score 500-649: Fair — manual review required above R$ 5,000
    D = 4,   // Score 350-499: Poor — manual review always required
    E = 5    // Score < 350: Very Poor — auto-reject
}
