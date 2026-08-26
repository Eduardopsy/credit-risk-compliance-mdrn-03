// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Fakes/FakeCreditProposalRepository.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Domain.Repositories;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Fakes;

/// <summary>
/// In-memory fake implementation of ICreditProposalRepository for unit tests.
/// Stores proposals in a dictionary. Thread-safe for single-threaded test execution.
/// </summary>
internal sealed class FakeCreditProposalRepository : ICreditProposalRepository
{
    private readonly Dictionary<Guid, CreditProposal> _store = [];

    public Task<CreditProposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue(id, out var proposal) ? proposal : null);
    }

    public Task<IReadOnlyList<CreditProposal>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<CreditProposal> result = _store.Values
            .Where(p => p.CustomerId == customerId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<CreditProposal>> GetByStatusAsync(ProposalStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<CreditProposal> result = _store.Values
            .Where(p => p.Status == status)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task AddAsync(CreditProposal proposal, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store[proposal.Id] = proposal;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CreditProposal proposal, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store[proposal.Id] = proposal;
        return Task.CompletedTask;
    }

    public Task<int> CountByStatusAsync(ProposalStatus status, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.Values.Count(p => p.Status == status));
    }

    // Test helpers
    public IReadOnlyList<CreditProposal> GetAll() => _store.Values.ToList().AsReadOnly();

    public void Clear() => _store.Clear();
}
