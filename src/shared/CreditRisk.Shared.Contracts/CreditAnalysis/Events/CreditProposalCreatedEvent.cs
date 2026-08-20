// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>
/// Published when a credit proposal is created and saved.
/// Consumed by: CreditAnalysis.Worker (to start evaluation pipeline).
/// </summary>
public sealed record CreditProposalCreatedEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }   // CPF or CNPJ (digits only)
    public required string CustomerDocumentType { get; init; } // "CPF" or "CNPJ"
    public required decimal RequestedLimit { get; init; }
    public required string ProposalType { get; init; }       // "Individual" or "LegalEntity"
    public required DateTimeOffset CreatedAt { get; init; }
    public required string CreatedBy { get; init; }          // Operator user ID
    public required Guid CorrelationId { get; init; }
}
