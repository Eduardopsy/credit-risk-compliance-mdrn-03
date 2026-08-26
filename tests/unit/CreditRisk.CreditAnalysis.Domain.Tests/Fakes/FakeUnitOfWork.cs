// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Fakes/FakeUnitOfWork.cs
using CreditRisk.CreditAnalysis.Application.Ports;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Fakes;

/// <summary>
/// Fake UnitOfWork that records commit calls for assertion in tests.
/// Does not dispatch domain events — tests verify domain state directly.
/// </summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int CommitCallCount { get; private set; }
    public bool ShouldThrow { get; set; }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ShouldThrow)
            throw new InvalidOperationException("Simulated database failure.");

        CommitCallCount++;
        return Task.FromResult(1);
    }

    // Test helpers
    public void Reset()
    {
        CommitCallCount = 0;
        ShouldThrow = false;
    }
}
