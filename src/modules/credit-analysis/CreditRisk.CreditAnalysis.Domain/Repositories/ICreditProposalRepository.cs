// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Repositories/ICreditProposalRepository.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;

namespace CreditRisk.CreditAnalysis.Domain.Repositories;

public interface ICreditProposalRepository
{
    Task<CreditProposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditProposal>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditProposal>> GetByStatusAsync(ProposalStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(CreditProposal proposal, CancellationToken cancellationToken = default);
    Task UpdateAsync(CreditProposal proposal, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(ProposalStatus status, CancellationToken cancellationToken = default);
}
