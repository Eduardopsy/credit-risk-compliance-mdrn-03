// File: tests/unit/CreditRisk.Compliance.Domain.Tests/Fakes/FakeAmlAlertRepository.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Enums;
using CreditRisk.Compliance.Domain.Repositories;

namespace CreditRisk.Compliance.Domain.Tests.Fakes;

/// <summary>
/// In-memory fake implementation of IAmlAlertRepository for unit tests.
/// </summary>
internal sealed class FakeAmlAlertRepository : IAmlAlertRepository
{
    private readonly Dictionary<Guid, AmlAlert> _storeById = [];
    private readonly List<AmlAlert> _allAlerts = [];

    public Task<AmlAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_storeById.TryGetValue(id, out var alert) ? alert : null);
    }

    public Task<IReadOnlyList<AmlAlert>> GetByStatusAsync(AlertStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<AmlAlert> result = _allAlerts
            .Where(a => a.Status == status)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task AddAsync(AmlAlert alert, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _storeById[alert.Id] = alert;
        _allAlerts.Add(alert);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AmlAlert alert, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _storeById[alert.Id] = alert;
        return Task.CompletedTask;
    }

    // Test helpers
    public IReadOnlyList<AmlAlert> GetAll() => _allAlerts.AsReadOnly();

    public void Clear()
    {
        _storeById.Clear();
        _allAlerts.Clear();
    }
}
