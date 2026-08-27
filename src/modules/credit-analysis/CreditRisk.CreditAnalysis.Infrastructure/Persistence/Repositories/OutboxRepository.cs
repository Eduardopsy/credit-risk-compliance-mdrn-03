// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/Repositories/OutboxRepository.cs
using CreditRisk.Shared.Kernel.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for persisting and retrieving outbox messages in PostgreSQL.
/// Implements IOutboxRepository for use by OutboxProcessor.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly CreditAnalysisDbContext _dbContext;

    public OutboxRepository(CreditAnalysisDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>Add a new message to the outbox for guaranteed delivery.</summary>
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        _dbContext.OutboxMessages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Get all unprocessed messages scheduled for now or earlier.</summary>
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int maxRetries = 5, int limit = 100, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var messages = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null && x.RetryCount < maxRetries && x.ScheduledAt <= now)
            .OrderBy(x => x.ScheduledAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return messages.AsReadOnly();
    }

    /// <summary>Update outbox message status after publish attempt.</summary>
    public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        _dbContext.OutboxMessages.Update(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Delete successfully processed messages older than specified age.</summary>
    public async Task<int> DeleteProcessedAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedAt.HasValue && x.ProcessedAt.Value < olderThan)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount;
    }
}
