// File: src/shared/CreditRisk.Shared.Kernel/Outbox/OutboxMessage.cs
using CreditRisk.Shared.Kernel.Domain;

namespace CreditRisk.Shared.Kernel.Outbox;

/// <summary>
/// Represents a message stored in the outbox table for guaranteed delivery.
/// Messages are written atomically with the business transaction and processed asynchronously by OutboxProcessor.
/// This implements the Outbox Pattern to ensure at-least-once delivery semantics.
/// </summary>
/// <remarks>
/// The Outbox Pattern solves the problem of maintaining consistency between the database and message broker:
/// 1. Business operation and outbox message written in same database transaction
/// 2. OutboxProcessor polls and publishes messages asynchronously
/// 3. If crash between SaveChanges and Publish, message remains in database and gets retried
/// 4. This prevents data inconsistency (committed state with no corresponding message)
/// </remarks>
public sealed class OutboxMessage : Entity
{
    /// <summary>Fully qualified type name of the message (for deserialization).</summary>
    public required string MessageType { get; init; }

    /// <summary>JSON payload of the message.</summary>
    public required string Payload { get; init; }

    /// <summary>When this message should be scheduled for processing.</summary>
    public required DateTimeOffset ScheduledAt { get; init; }

    /// <summary>When the message was successfully published. Null until processed.</summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    /// <summary>Number of publish attempts (for retry tracking).</summary>
    public int RetryCount { get; private set; }

    /// <summary>Error message if last publish attempt failed. Null if successful.</summary>
    public string? Error { get; private set; }

    private OutboxMessage() : base() { }

    /// <summary>
    /// Create a new outbox message for guaranteed delivery.
    /// </summary>
    /// <param name="messageType">Fully qualified type name for deserialization.</param>
    /// <param name="payload">JSON-serialized message content.</param>
    /// <returns>New OutboxMessage ready to be persisted.</returns>
    /// <exception cref="ArgumentException">If messageType or payload are empty/null.</exception>
    public static OutboxMessage Create(string messageType, string payload)
    {
        if (string.IsNullOrWhiteSpace(messageType))
            throw new ArgumentException("Message type cannot be empty.", nameof(messageType));
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload cannot be empty.", nameof(payload));

        return new OutboxMessage
        {
            MessageType = messageType,
            Payload = payload,
            ScheduledAt = DateTimeOffset.UtcNow,
            RetryCount = 0,
            Error = null,
            ProcessedAt = null
        };
    }

    /// <summary>Mark the message as successfully published.</summary>
    public void MarkProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = null;
    }

    /// <summary>Mark the message publish attempt as failed and increment retry counter.</summary>
    /// <param name="errorMessage">Description of the failure.</param>
    public void MarkFailed(string errorMessage)
    {
        Error = errorMessage;
        RetryCount++;
        ProcessedAt = null; // Reset processed status to retry
    }
}
