// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Services/ICreditScoringEngine.cs
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.Shared.Kernel.ValueObjects;

namespace CreditRisk.CreditAnalysis.Domain.Services;

public interface ICreditScoringEngine
{
    (RiskRating Rating, MoneyAmount ApprovedLimit, bool RequiresManualReview) Evaluate(
        decimal monthlyIncome,
        MoneyAmount requestedLimit,
        int bureauScore);
}
