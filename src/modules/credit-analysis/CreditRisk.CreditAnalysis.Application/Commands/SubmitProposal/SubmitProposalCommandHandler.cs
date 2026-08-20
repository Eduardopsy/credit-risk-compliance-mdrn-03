// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Commands/SubmitProposal/SubmitProposalCommandHandler.cs
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Application.Ports;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.CreditAnalysis.Application.Commands.SubmitProposal;

public sealed class SubmitProposalCommandHandler(
    ICreditProposalRepository proposalRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SubmitProposalCommand, CreditProposalDto>
{
    public async Task<Result<CreditProposalDto>> HandleAsync(SubmitProposalCommand command, CancellationToken cancellationToken = default)
    {
        var proposal = await proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken).ConfigureAwait(false);
        if (proposal is null)
        {
            return Result<CreditProposalDto>.Failure(Error.NotFound("Proposal.NotFound", $"Proposal {command.ProposalId} not found."));
        }

        proposal.Submit();
        await proposalRepository.UpdateAsync(proposal, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

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
