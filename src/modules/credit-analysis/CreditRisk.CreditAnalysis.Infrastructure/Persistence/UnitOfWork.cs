// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/UnitOfWork.cs
using CreditRisk.CreditAnalysis.Application.Ports;
using CreditRisk.Shared.Kernel.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence;

public sealed class UnitOfWork(
    CreditAnalysisDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        int rowsAffected = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entry in dbContext.ChangeTracker.Entries<AggregateRoot>())
            entry.Entity.ClearDomainEvents();

        return rowsAffected;
    }
}
