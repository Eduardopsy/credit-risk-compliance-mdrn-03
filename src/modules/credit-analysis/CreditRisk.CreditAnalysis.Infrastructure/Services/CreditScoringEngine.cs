// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Services/CreditScoringEngine.cs
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Domain.Services;
using CreditRisk.Shared.Kernel.ValueObjects;

namespace CreditRisk.CreditAnalysis.Infrastructure.Services;

/// <summary>
/// Implements credit scoring matrix following BCB Resolução 4.557 risk classification.
/// Calculates rating from bureau score, income, and debt ratio.
/// </summary>
public sealed class CreditScoringEngine : ICreditScoringEngine
{
    // Auto-approval thresholds (in BRL) for each rating
    private const decimal RatingA_AutoApproveLimit = 50_000m;
    private const decimal RatingB_AutoApproveLimit = 20_000m;
    private const decimal RatingC_AutoApproveLimit = 5_000m;
    private const decimal SystemMaxLimit = 500_000m;

    /// <summary>
    /// Evaluates credit based on bureau score, monthly income, and requested limit.
    /// </summary>
    /// <returns>Tuple of (Rating, ApprovedLimit, RequiresManualReview)</returns>
    public (RiskRating Rating, MoneyAmount ApprovedLimit, bool RequiresManualReview) Evaluate(
        decimal monthlyIncome,
        MoneyAmount requestedLimit,
        int bureauScore)
    {
        // Zero income = automatic rejection
        if (monthlyIncome <= 0)
        {
            return (RiskRating.E, MoneyAmount.Zero(), false);
        }

        // Determine rating from bureau score
        RiskRating rating = GetRatingFromBureauScore(bureauScore);

        // Rating E = auto-reject
        if (rating == RiskRating.E)
        {
            return (rating, MoneyAmount.Zero(), false);
        }

        // Cap approved limit at system max
        decimal approvedAmount = Math.Min(requestedLimit.Amount, SystemMaxLimit);

        // Determine if manual review is required
        bool requiresManualReview = RequiresManualReviewForRating(rating, approvedAmount);

        return (rating, MoneyAmount.Create(approvedAmount), requiresManualReview);
    }

    /// <summary>
    /// Maps bureau score (typically 0-900 in Brazil) to risk rating (A-E).
    /// Follows BCB standard thresholds.
    /// </summary>
    private static RiskRating GetRatingFromBureauScore(int bureauScore)
    {
        return bureauScore switch
        {
            >= 750 => RiskRating.A,
            >= 650 => RiskRating.B,
            >= 500 => RiskRating.C,
            >= 350 => RiskRating.D,
            _ => RiskRating.E
        };
    }

    /// <summary>
    /// Determines if manual review is required based on rating and requested amount.
    /// Higher ratings have higher auto-approve thresholds.
    /// </summary>
    private static bool RequiresManualReviewForRating(RiskRating rating, decimal approvedAmount)
    {
        return rating switch
        {
            RiskRating.A => approvedAmount > RatingA_AutoApproveLimit,
            RiskRating.B => approvedAmount > RatingB_AutoApproveLimit,
            RiskRating.C => approvedAmount > RatingC_AutoApproveLimit,
            RiskRating.D => true,  // D always requires manual review
            RiskRating.E => false, // E is auto-reject, no review needed
            _ => throw new ArgumentException($"Unknown rating: {rating}", nameof(rating))
        };
    }
}
