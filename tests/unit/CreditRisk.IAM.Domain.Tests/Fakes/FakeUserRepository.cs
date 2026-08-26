// File: tests/unit/CreditRisk.IAM.Domain.Tests/Fakes/FakeUserRepository.cs
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.IAM.Domain.Repositories;
using CreditRisk.IAM.Domain.ValueObjects;

namespace CreditRisk.IAM.Domain.Tests.Fakes;

/// <summary>
/// In-memory fake implementation of IUserRepository for unit tests.
/// Stores users in a dictionary. Thread-safe for single-threaded test execution.
/// </summary>
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _storeById = [];
    private readonly Dictionary<string, User> _storeByEmail = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_storeById.TryGetValue(id, out var user) ? user : null);
    }

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_storeByEmail.TryGetValue(email.Value, out var user) ? user : null);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _storeById[user.Id] = user;
        _storeByEmail[user.Email.Value] = user;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _storeById[user.Id] = user;
        _storeByEmail[user.Email.Value] = user;
        return Task.CompletedTask;
    }

    // Test helpers
    public void Add(User user)
    {
        _storeById[user.Id] = user;
        _storeByEmail[user.Email.Value] = user;
    }

    public IReadOnlyList<User> GetAll() => _storeById.Values.ToList().AsReadOnly();

    public void Clear()
    {
        _storeById.Clear();
        _storeByEmail.Clear();
    }
}
