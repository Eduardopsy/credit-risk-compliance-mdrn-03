// File: src/shared/CreditRisk.Shared.Kernel/Outbox/IOutboxRepository.cs
namespace CreditRisk.Shared.Kernel.Outbox;

/// <summary>
/// Repository for persisting and retrieving outbox messages.
/// Ensures atomic transactions between business operations and message publication.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>Add a new message to the outbox for guaranteed delivery.</summary>
    /// <param name="message">The outbox message to persist.</param>
    /// <returns>Completed task.</returns>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>Get all unprocessed messages scheduled for now or earlier.</summary>
    /// <param name="maxRetries">Messages with retry count >= maxRetries are excluded.</param>
    /// <param name="limit">Maximum number of messages to retrieve.</param>
    /// <returns>Collection of messages ready to be published.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int maxRetries = 5, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>Update outbox message status after publish attempt.</summary>
    /// <param name="message">The message with updated status.</param>
    /// <returns>Completed task.</returns>
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>Delete successfully processed messages older than specified age.</summary>
    /// <param name="olderThan">Delete messages processed before this timestamp.</param>
    /// <returns>Number of messages deleted.</returns>
    Task<int> DeleteProcessedAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default);
}
