// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/DTOs/ProposalListItemDto.cs
namespace CreditRisk.CreditAnalysis.Application.DTOs;

public sealed record ProposalListItemDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal RequestedLimit { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
