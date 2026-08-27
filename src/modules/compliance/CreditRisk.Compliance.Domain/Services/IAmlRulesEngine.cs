// File: src/modules/compliance/CreditRisk.Compliance.Domain/Services/IAmlRulesEngine.cs
using CreditRisk.Compliance.Domain.Enums;

namespace CreditRisk.Compliance.Domain.Services;

/// <summary>
/// Evaluates AML (Anti-Money Laundering) rules for suspicious transaction patterns.
/// Implements behavioral analysis and threshold-based detection.
/// </summary>
public interface IAmlRulesEngine
{
    /// <summary>
    /// Evaluates whether a transaction triggers AML rules.
    /// </summary>
    /// <param name="customerId">Customer performing the transaction.</param>
    /// <param name="amount">Transaction amount in currency units.</param>
    /// <param name="transactionType">Type of transaction (Online, ATM, Transfer, etc.).</param>
    /// <returns>Alert rule info if triggered; null if no alert.</returns>
    Task<AmlRuleAlert?> EvaluateTransactionAsync(Guid customerId, decimal amount, string transactionType);
}

/// <summary>
/// Represents an AML rule alert when a rule is triggered.
/// </summary>
public sealed record AmlRuleAlert(
    string RuleType,              // e.g., "Structuring", "LargeTransaction", "UnusualPattern"
    AlertSeverity Severity,       // Low, Medium, High, Critical
    string Description);          // Human-readable explanation
