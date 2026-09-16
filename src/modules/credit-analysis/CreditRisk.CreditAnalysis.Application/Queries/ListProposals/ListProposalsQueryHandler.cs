// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Queries/ListProposals/ListProposalsQueryHandler.cs
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using CreditRisk.Shared.Kernel.Result;
using Microsoft.Extensions.Logging;

namespace CreditRisk.CreditAnalysis.Application.Queries.ListProposals;

public sealed class ListProposalsQueryHandler(
    ICreditProposalRepository proposalRepository,
    ILogger<ListProposalsQueryHandler> logger)
{
    public async Task<Result<PagedResult<ProposalListItemDto>>> HandleAsync(
        ListProposalsQuery query,
        CancellationToken cancellationToken = default)
    {
        int page = query.Page < 1 ? 1 : query.Page;
        int pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        int totalCount = await proposalRepository.CountAsync(cancellationToken).ConfigureAwait(false);
        var proposals = await proposalRepository.ListAsync(page, pageSize, cancellationToken).ConfigureAwait(false);

        var items = proposals.Select(p => new ProposalListItemDto
        {
            Id = p.Id,
            CustomerId = p.CustomerId,
            RequestedLimit = p.RequestedLimit.Amount,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt
        }).ToList();

        var paged = new PagedResult<ProposalListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Result<PagedResult<ProposalListItemDto>>.Success(paged);
    }
}
