// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Commands/TransactionReceivedCommand.cs
namespace CreditRisk.Shared.Contracts.Compliance.Commands;

/// <summary>Command published by Compliance API for async AML processing.</summary>
public sealed record TransactionReceivedCommand
{
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }
    public required decimal Amount { get; init; }
    public required string TransactionType { get; init; }    // "Credit" | "Debit" | "Transfer"
    public required string Channel { get; init; }            // "ATM" | "Online" | "Branch" | "Mobile"
    public required DateTimeOffset TransactionDate { get; init; }
    public required string OriginAccountId { get; init; }
    public required string DestinationAccountId { get; init; }
    public required Guid CorrelationId { get; init; }
}
