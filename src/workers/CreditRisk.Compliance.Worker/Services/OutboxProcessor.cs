// File: src/workers/CreditRisk.Compliance.Worker/Services/OutboxProcessor.cs
using CreditRisk.Shared.Kernel.Outbox;
using MassTransit;
using System.Text.Json;

namespace CreditRisk.Compliance.Worker.Services;

/// <summary>
/// Background service that processes outbox messages and publishes them to MassTransit.
/// Polls every 5 seconds, retrieves unprocessed messages, and publishes them with retry logic.
/// Implements guaranteed delivery semantics for event-sourcing patterns.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollInterval;
    private const int MaxRetries = 5;
    private const int BatchSize = 100;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        IOutboxRepository outboxRepository,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _outboxRepository = outboxRepository;
        _logger = logger;
        _pollInterval = TimeSpan.FromSeconds(5);
    }

    /// <summary>Execute the background service continuously.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor starting. Poll interval: {PollInterval} seconds", _pollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Outbox processor stopping");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        var messages = await _outboxRepository.GetUnprocessedAsync(MaxRetries, BatchSize, cancellationToken);

        if (messages.Count == 0)
            return;

        _logger.LogDebug("Processing {MessageCount} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            await PublishMessageAsync(message, cancellationToken);
        }

        // Cleanup old processed messages (older than 30 days)
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);
        var deletedCount = await _outboxRepository.DeleteProcessedAsync(cutoffDate, cancellationToken);
        if (deletedCount > 0)
            _logger.LogDebug("Deleted {DeletedCount} processed outbox messages", deletedCount);
    }

    private async Task PublishMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var messageType = Type.GetType(message.MessageType);
            if (messageType is null)
            {
                var error = $"Message type '{message.MessageType}' not found. Skipping message.";
                _logger.LogError("Outbox message {MessageId} deserialization failed: {Error}", message.Id, error);
                message.MarkFailed(error);
                await _outboxRepository.UpdateAsync(message, cancellationToken);
                return;
            }

            var payload = JsonSerializer.Deserialize(message.Payload, messageType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload is null)
            {
                var error = "Failed to deserialize payload";
                _logger.LogError("Outbox message {MessageId} payload deserialization failed", message.Id);
                message.MarkFailed(error);
                await _outboxRepository.UpdateAsync(message, cancellationToken);
                return;
            }

            // Publish to MassTransit
            await using var scope = _serviceProvider.CreateAsyncScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(payload, messageType, cancellationToken);

            message.MarkProcessed();
            await _outboxRepository.UpdateAsync(message, cancellationToken);
            _logger.LogDebug("Published outbox message {MessageId} of type {MessageType}", message.Id, message.MessageType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish outbox message {MessageId}, retry count: {RetryCount}", message.Id, message.RetryCount);
            message.MarkFailed(ex.Message);
            await _outboxRepository.UpdateAsync(message, cancellationToken);
        }
    }
}
