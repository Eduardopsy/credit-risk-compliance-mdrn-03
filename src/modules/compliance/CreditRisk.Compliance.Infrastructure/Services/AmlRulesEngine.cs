// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Services/AmlRulesEngine.cs
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Compliance.Domain.Services;
using CreditRisk.Compliance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CreditRisk.Compliance.Infrastructure.Services;

/// <summary>
/// Implements AML (Anti-Money Laundering) rule evaluation.
/// Detects suspicious patterns: structuring, large transactions, unusual activity.
/// </summary>
public sealed class AmlRulesEngine : IAmlRulesEngine
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<AmlRulesEngine> _logger;

    // AML Rule Thresholds
    private const decimal StructuringThreshold = 10_000m;        // Below reporting threshold
    private const decimal LargeTransactionThreshold = 100_000m;  // Unusual amount
    private const int StructuringWindowDays = 7;                 // Check within 7 days
    private const int MaxTransactionsPerDay = 15;                // Rapid transaction detection

    public AmlRulesEngine(
        ITransactionRepository transactionRepository,
        ILogger<AmlRulesEngine> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates transaction against AML rules.
    /// Returns alert if any rule is triggered.
    /// </summary>
    public async Task<AmlRuleAlert?> EvaluateTransactionAsync(Guid customerId, decimal amount, string transactionType)
    {
        _logger.LogInformation(
            "Evaluating AML rules CustomerId={CustomerId} Amount={Amount} Type={TransactionType}",
            customerId, amount, transactionType);

        // Rule 1: Large Transaction Detection
        if (amount > LargeTransactionThreshold)
        {
            _logger.LogWarning(
                "Large transaction detected CustomerId={CustomerId} Amount={Amount}",
                customerId, amount);

            return new AmlRuleAlert(
                RuleType: "LargeTransaction",
                Severity: AlertSeverity.High,
                Description: $"Transaction amount {amount:C} exceeds large transaction threshold");
        }

        // Rule 2: Structuring Detection (multiple small transactions)
        if (amount > 0 && amount < StructuringThreshold)
        {
            var recentTransactions = await _transactionRepository.GetByCustomerIdAsync(
                customerId,
                limit: 100,
                cancellationToken: CancellationToken.None);

            var windowStart = DateTimeOffset.UtcNow.AddDays(-StructuringWindowDays);
            var structuringTransactions = recentTransactions
                .Where(t => t.TransactionDate >= windowStart && t.Amount.Amount < StructuringThreshold)
                .ToList();

            // Alert if we detect 5+ small transactions in 7 days (potential structuring)
            if (structuringTransactions.Count >= 5)
            {
                _logger.LogWarning(
                    "Potential structuring detected CustomerId={CustomerId} TransactionCount={Count}",
                    customerId, structuringTransactions.Count);

                return new AmlRuleAlert(
                    RuleType: "Structuring",
                    Severity: AlertSeverity.Medium,
                    Description: $"Detected {structuringTransactions.Count} small transactions in {StructuringWindowDays} days - potential structuring pattern");
            }
        }

        // Rule 3: Rapid Transaction Detection
        var today = DateTimeOffset.UtcNow.Date;
        var todayTransactions = await _transactionRepository.GetByCustomerIdAsync(
            customerId,
            limit: 100,
            cancellationToken: CancellationToken.None);

        var transactionsToday = todayTransactions
            .Count(t => t.TransactionDate.Date == today);

        if (transactionsToday > MaxTransactionsPerDay)
        {
            _logger.LogWarning(
                "Unusual transaction frequency detected CustomerId={CustomerId} TransactionCount={Count}",
                customerId, transactionsToday);

            return new AmlRuleAlert(
                RuleType: "UnusualFrequency",
                Severity: AlertSeverity.Medium,
                Description: $"Customer has {transactionsToday} transactions today - unusual activity");
        }

        _logger.LogInformation(
            "AML rules evaluation passed CustomerId={CustomerId}",
            customerId);

        return null; // No alert
    }
}
