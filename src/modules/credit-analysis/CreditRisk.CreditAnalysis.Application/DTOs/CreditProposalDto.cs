// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/DTOs/CreditProposalDto.cs
namespace CreditRisk.CreditAnalysis.Application.DTOs;

public sealed record CreditProposalDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }
    public required decimal RequestedLimit { get; init; }
    public required decimal? ApprovedLimit { get; init; }
    public required string Status { get; init; }
    public required string? RiskRating { get; init; }
    public required bool RequiresManualReview { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required string CreatedBy { get; init; }
}
