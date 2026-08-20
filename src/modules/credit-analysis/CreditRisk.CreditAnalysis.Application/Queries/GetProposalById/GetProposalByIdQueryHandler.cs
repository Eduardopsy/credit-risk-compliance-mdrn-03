// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Queries/GetProposalById/GetProposalByIdQueryHandler.cs
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.CreditAnalysis.Application.Queries.GetProposalById;

public sealed class GetProposalByIdQueryHandler(ICreditProposalRepository proposalRepository) : IQueryHandler<GetProposalByIdQuery, CreditProposalDto>
{
    public async Task<Result<CreditProposalDto>> HandleAsync(GetProposalByIdQuery query, CancellationToken cancellationToken = default)
    {
        var proposal = await proposalRepository.GetByIdAsync(query.ProposalId, cancellationToken).ConfigureAwait(false);
        if (proposal is null)
        {
            return Result<CreditProposalDto>.Failure(Error.NotFound("Proposal.NotFound", $"Proposal {query.ProposalId} not found."));
        }

        return Result<CreditProposalDto>.Success(new CreditProposalDto
        {
            Id = proposal.Id,
            CustomerId = proposal.CustomerId,
            CustomerDocument = "12345678909",
            RequestedLimit = proposal.RequestedLimit.Amount,
            ApprovedLimit = proposal.ApprovedLimit?.Amount,
            Status = proposal.Status.ToString(),
            RiskRating = proposal.Rating?.ToString(),
            RequiresManualReview = proposal.RequiresManualReview,
            CreatedAt = proposal.CreatedAt,
            UpdatedAt = proposal.UpdatedAt,
            CreatedBy = proposal.CreatedBy
        });
    }
}
