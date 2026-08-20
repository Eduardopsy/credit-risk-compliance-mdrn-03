// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/Repositories/CreditProposalRepository.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence.Repositories;

public sealed class CreditProposalRepository(CreditAnalysisDbContext context) : ICreditProposalRepository
{
    public async Task<CreditProposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.CreditProposals.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CreditProposal>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await context.CreditProposals
            .Where(p => p.CustomerId == customerId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CreditProposal>> GetByStatusAsync(ProposalStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await context.CreditProposals
            .Where(p => p.Status == status)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(CreditProposal proposal, CancellationToken cancellationToken = default)
    {
        await context.CreditProposals.AddAsync(proposal, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(CreditProposal proposal, CancellationToken cancellationToken = default)
    {
        context.CreditProposals.Update(proposal);
        await Task.CompletedTask;
    }

    public async Task<int> CountByStatusAsync(ProposalStatus status, CancellationToken cancellationToken = default)
    {
        return await context.CreditProposals
            .CountAsync(p => p.Status == status, cancellationToken)
            .ConfigureAwait(false);
    }
}
