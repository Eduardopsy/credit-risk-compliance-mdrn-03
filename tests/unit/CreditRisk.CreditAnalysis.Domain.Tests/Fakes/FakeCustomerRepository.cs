// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Fakes/FakeCustomerRepository.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Repositories;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Fakes;

/// <summary>
/// In-memory fake implementation of ICustomerRepository for unit tests.
/// </summary>
internal sealed class FakeCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<Guid, Customer> _storeById = [];
    private readonly Dictionary<string, Customer> _storeByDocument = [];

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_storeById.TryGetValue(id, out var customer) ? customer : null);
    }

    public Task<Customer?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_storeByDocument.TryGetValue(document, out var customer) ? customer : null);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _storeById[customer.Id] = customer;
        return Task.CompletedTask;
    }

    // Test helpers
    public void Add(Customer customer)
    {
        _storeById[customer.Id] = customer;
    }

    public IReadOnlyList<Customer> GetAll() => _storeById.Values.ToList().AsReadOnly();

    public void Clear() => _storeById.Clear();
}
