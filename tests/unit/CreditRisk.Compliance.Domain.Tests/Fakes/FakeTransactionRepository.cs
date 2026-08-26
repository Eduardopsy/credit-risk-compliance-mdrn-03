// File: tests/unit/CreditRisk.Compliance.Domain.Tests/Fakes/FakeTransactionRepository.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Repositories;

namespace CreditRisk.Compliance.Domain.Tests.Fakes;

/// <summary>
/// In-memory fake implementation of ITransactionRepository for unit tests.
/// Stores transactions in a list. Supports querying by customer ID for smurfing pattern detection.
/// </summary>
internal sealed class FakeTransactionRepository : ITransactionRepository
{
    private readonly Dictionary<Guid, Transaction> _storeById = [];
    private readonly List<Transaction> _allTransactions = [];
    private readonly HashSet<string> _pepDocuments = [];

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_storeById.TryGetValue(id, out var transaction) ? transaction : null);
    }

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _storeById[transaction.Id] = transaction;
        _allTransactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _storeById[transaction.Id] = transaction;
        return Task.CompletedTask;
    }

    // Test helpers
    public IReadOnlyList<Transaction> GetByCustomerId(Guid customerId)
        => _allTransactions.Where(t => t.CustomerId == customerId).ToList().AsReadOnly();

    public IReadOnlyList<Transaction> GetRecentByCustomerId(Guid customerId, TimeSpan within)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(within);
        return _allTransactions
            .Where(t => t.CustomerId == customerId && t.CreatedAt >= cutoff)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Transaction> GetAll() => _allTransactions.AsReadOnly();

    public void AddPepDocument(string document) => _pepDocuments.Add(document);

    public bool IsPepDocument(string document) => _pepDocuments.Contains(document);

    public void Clear()
    {
        _storeById.Clear();
        _allTransactions.Clear();
        _pepDocuments.Clear();
    }
}
