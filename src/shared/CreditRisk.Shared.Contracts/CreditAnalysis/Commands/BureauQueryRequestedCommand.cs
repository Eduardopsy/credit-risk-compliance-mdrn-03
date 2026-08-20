// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Commands/BureauQueryRequestedCommand.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Commands;

/// <summary>Command to trigger a credit bureau query for a customer.</summary>
public sealed record BureauQueryRequestedCommand
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }
    public required string CustomerDocumentType { get; init; }
    public required Guid CorrelationId { get; init; }
}
