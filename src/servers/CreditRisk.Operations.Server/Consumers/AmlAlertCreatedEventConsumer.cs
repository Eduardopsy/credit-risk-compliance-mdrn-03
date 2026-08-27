// File: src/servers/CreditRisk.Operations.Server/Consumers/AmlAlertCreatedEventConsumer.cs
using CreditRisk.Operations.Server.Services;
using CreditRisk.Shared.Contracts.Compliance.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CreditRisk.Operations.Server.Consumers;

/// <summary>
/// Consumes AmlAlertCreatedEvent and broadcasts to connected clients via SignalR.
/// Triggered by compliance worker when alerts are created and published.
/// </summary>
public sealed class AmlAlertCreatedEventConsumer : IConsumer<AmlAlertCreatedEvent>
{
    private readonly ILogger<AmlAlertCreatedEventConsumer> _logger;
    private readonly AlertNotificationService _notificationService;

    public AmlAlertCreatedEventConsumer(
        ILogger<AmlAlertCreatedEventConsumer> logger,
        AlertNotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// Handles incoming AmlAlertCreatedEvent.
    /// Broadcasts to operations dashboard via SignalR hub.
    /// </summary>
    public async Task Consume(ConsumeContext<AmlAlertCreatedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Processing AmlAlertCreatedEvent AlertId={AlertId} TransactionId={TransactionId} Severity={Severity} CorrelationId={CorrelationId}",
            evt.AlertId, evt.TransactionId, evt.Severity, evt.CorrelationId);

        try
        {
            // Broadcast alert to all connected clients
            await _notificationService.BroadcastAmlAlertAsync(
                alertId: evt.AlertId,
                transactionId: evt.TransactionId,
                customerId: evt.CustomerId,
                alertType: evt.AlertType,
                severity: evt.Severity,
                transactionAmount: evt.TransactionAmount,
                createdAt: evt.CreatedAt);

            _logger.LogInformation(
                "AmlAlertCreatedEvent broadcasted successfully AlertId={AlertId}",
                evt.AlertId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AmlAlertCreatedEvent AlertId={AlertId}", evt.AlertId);
            throw;
        }
    }
}
