// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Services/CreditScoringEngineTests.cs
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Infrastructure.Services;
using CreditRisk.Shared.Kernel.ValueObjects;
using FluentAssertions;
using Xunit;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Services;

public sealed class CreditScoringEngineTests
{
    private readonly CreditScoringEngine _engine = new();

    // ── Rating A Tests ──────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_ExcellentBureauLowDebtLowMultiplier_ReturnsRatingA()
    {
        // Arrange: Bureau score 850, income 10k, debt 1k = excellent profile
        var requestedLimit = MoneyAmount.Create(15_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 10_000m,
            requestedLimit: requestedLimit,
            bureauScore: 850);

        // Assert
        result.Rating.Should().Be(RiskRating.A);
    }

    [Fact]
    public void Evaluate_RatingA_BelowAutoApproveThreshold_RequiresManualReviewIsFalse()
    {
        // Arrange: Rating A but small requested amount (auto-approve threshold)
        var requestedLimit = MoneyAmount.Create(30_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 20_000m,
            requestedLimit: requestedLimit,
            bureauScore: 900);

        // Assert
        result.Rating.Should().Be(RiskRating.A);
        result.RequiresManualReview.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_RatingA_AboveAutoApproveThreshold_RequiresManualReviewIsTrue()
    {
        // Arrange: Rating A but large requested amount
        var requestedLimit = MoneyAmount.Create(80_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 50_000m,
            requestedLimit: requestedLimit,
            bureauScore: 900);

        // Assert
        result.Rating.Should().Be(RiskRating.A);
        result.RequiresManualReview.Should().BeTrue();
    }

    // ── Rating B Tests ──────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_GoodBureauModerateDebt_ReturnsRatingB()
    {
        // Arrange: Bureau score 700 = good profile
        var requestedLimit = MoneyAmount.Create(15_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 8_000m,
            requestedLimit: requestedLimit,
            bureauScore: 700);

        // Assert
        result.Rating.Should().Be(RiskRating.B);
    }

    [Fact]
    public void Evaluate_RatingB_BelowAutoApproveThreshold_RequiresManualReviewIsFalse()
    {
        // Arrange: Rating B with reasonable limit
        var requestedLimit = MoneyAmount.Create(15_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 10_000m,
            requestedLimit: requestedLimit,
            bureauScore: 700);

        // Assert
        result.Rating.Should().Be(RiskRating.B);
        result.RequiresManualReview.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_RatingB_AboveAutoApproveThreshold_RequiresManualReviewIsTrue()
    {
        // Arrange: Rating B with high requested limit
        var requestedLimit = MoneyAmount.Create(25_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 20_000m,
            requestedLimit: requestedLimit,
            bureauScore: 700);

        // Assert
        result.Rating.Should().Be(RiskRating.B);
        result.RequiresManualReview.Should().BeTrue();
    }

    // ── Rating C Tests ──────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_FairBureauHighDebt_ReturnsRatingC()
    {
        // Arrange: Bureau score 580 = fair profile
        var requestedLimit = MoneyAmount.Create(20_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 6_000m,
            requestedLimit: requestedLimit,
            bureauScore: 580);

        // Assert
        result.Rating.Should().Be(RiskRating.C);
    }

    [Fact]
    public void Evaluate_RatingC_BelowAutoApproveThreshold_RequiresManualReviewIsFalse()
    {
        // Arrange: Rating C with low requested limit (auto-approve for small amounts)
        var requestedLimit = MoneyAmount.Create(4_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 6_000m,
            requestedLimit: requestedLimit,
            bureauScore: 580);

        // Assert
        result.Rating.Should().Be(RiskRating.C);
        result.RequiresManualReview.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_RatingC_AboveAutoApproveThreshold_RequiresManualReviewIsTrue()
    {
        // Arrange: Rating C with high requested limit (requires review)
        var requestedLimit = MoneyAmount.Create(8_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 6_000m,
            requestedLimit: requestedLimit,
            bureauScore: 580);

        // Assert
        result.Rating.Should().Be(RiskRating.C);
        result.RequiresManualReview.Should().BeTrue();
    }

    // ── Rating D Tests ──────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_PoorBureauHighDebt_ReturnsRatingD()
    {
        // Arrange: Bureau score 400 = poor profile
        var requestedLimit = MoneyAmount.Create(25_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 5_000m,
            requestedLimit: requestedLimit,
            bureauScore: 400);

        // Assert
        result.Rating.Should().Be(RiskRating.D);
    }

    [Fact]
    public void Evaluate_RatingD_AlwaysRequiresManualReview_EvenForSmallAmounts()
    {
        // Arrange: Rating D always requires manual review
        var requestedLimit = MoneyAmount.Create(100m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 5_000m,
            requestedLimit: requestedLimit,
            bureauScore: 400);

        // Assert
        result.Rating.Should().Be(RiskRating.D);
        result.RequiresManualReview.Should().BeTrue();
    }

    // ── Rating E Tests ──────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_VeryPoorBureauVeryHighDebt_ReturnsRatingE()
    {
        // Arrange: Bureau score 200 = very poor profile
        var requestedLimit = MoneyAmount.Create(30_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 3_000m,
            requestedLimit: requestedLimit,
            bureauScore: 200);

        // Assert
        result.Rating.Should().Be(RiskRating.E);
    }

    [Fact]
    public void Evaluate_RatingE_ApprovedLimitIsZero()
    {
        // Arrange: Rating E = auto-reject (approved limit 0)
        var requestedLimit = MoneyAmount.Create(50_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 2_000m,
            requestedLimit: requestedLimit,
            bureauScore: 100);

        // Assert
        result.Rating.Should().Be(RiskRating.E);
        result.ApprovedLimit.Amount.Should().Be(0m);
        result.RequiresManualReview.Should().BeFalse();
    }

    // ── Boundary Tests ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(850, RiskRating.A)]   // Rating A threshold
    [InlineData(700, RiskRating.B)]   // Rating B threshold
    [InlineData(580, RiskRating.C)]   // Rating C threshold
    [InlineData(400, RiskRating.D)]   // Rating D threshold
    [InlineData(200, RiskRating.E)]   // Rating E threshold
    public void Evaluate_BoundaryBureauScores_ReturnsCorrectRating(int bureauScore, RiskRating expectedRating)
    {
        // Arrange
        var requestedLimit = MoneyAmount.Create(10_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 5_000m,
            requestedLimit: requestedLimit,
            bureauScore: bureauScore);

        // Assert
        result.Rating.Should().Be(expectedRating);
    }

    // ── Edge Cases ──────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_ZeroMonthlyIncome_ReturnsRatingE()
    {
        // Arrange: No income = cannot service any debt
        var requestedLimit = MoneyAmount.Create(10_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 0m,
            requestedLimit: requestedLimit,
            bureauScore: 800);

        // Assert
        result.Rating.Should().Be(RiskRating.E);
    }

    [Fact]
    public void Evaluate_RequestedLimitExceedsSystemMax_ApprovedLimitCappedAt500k()
    {
        // Arrange: System max limit is 500k
        var requestedLimit = MoneyAmount.Create(600_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 500_000m,
            requestedLimit: requestedLimit,
            bureauScore: 900);

        // Assert
        result.Rating.Should().Be(RiskRating.A);
        result.ApprovedLimit.Amount.Should().BeLessThanOrEqualTo(500_000m);
    }

    [Fact]
    public void Evaluate_VeryHighIncome_StillRespectsBureauScore()
    {
        // Arrange: High income doesn't override poor bureau score
        var requestedLimit = MoneyAmount.Create(100_000m);

        // Act
        var result = _engine.Evaluate(
            monthlyIncome: 1_000_000m,  // Very high income
            requestedLimit: requestedLimit,
            bureauScore: 200);          // But very poor bureau score

        // Assert
        result.Rating.Should().Be(RiskRating.E);
    }
}
