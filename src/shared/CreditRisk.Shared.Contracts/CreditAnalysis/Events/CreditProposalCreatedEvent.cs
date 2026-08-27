// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>
/// Published when a credit proposal is created.
/// Triggers credit analysis workflow: bureau query → scoring → evaluation.
/// </summary>
public sealed record CreditProposalCreatedEvent
{
    /// <summary>Unique proposal identifier.</summary>
    public required Guid ProposalId { get; init; }

    /// <summary>Customer requesting the credit.</summary>
    public required Guid CustomerId { get; init; }

    /// <summary>Customer's document (CPF/CNPJ).</summary>
    public required string CustomerDocument { get; init; }

    /// <summary>Type of document (CPF, CNPJ, etc.).</summary>
    public required string CustomerDocumentType { get; init; }

    /// <summary>Amount requested for credit line.</summary>
    public required decimal RequestedLimit { get; init; }

    /// <summary>Type of proposal (Individual, Business, etc.).</summary>
    public required string ProposalType { get; init; }

    /// <summary>When the proposal was created.</summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>Who created the proposal.</summary>
    public required string CreatedBy { get; init; }

    /// <summary>Correlation ID for distributed tracing.</summary>
    public required Guid CorrelationId { get; init; }
}
