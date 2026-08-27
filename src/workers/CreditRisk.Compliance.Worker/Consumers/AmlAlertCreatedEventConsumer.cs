// File: src/workers/CreditRisk.Compliance.Worker/Consumers/AmlAlertCreatedEventConsumer.cs
using CreditRisk.Shared.Contracts.Compliance.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CreditRisk.Compliance.Worker.Consumers;

/// <summary>
/// Consumes AmlAlertCreatedEvent and handles operations notifications.
/// May trigger escalation, compliance review, or regulatory reporting workflows.
/// In this implementation, it logs the event for compliance audit trail.
/// </summary>
public sealed class AmlAlertCreatedEventConsumer : IConsumer<AmlAlertCreatedEvent>
{
    private readonly ILogger<AmlAlertCreatedEventConsumer> _logger;

    public AmlAlertCreatedEventConsumer(ILogger<AmlAlertCreatedEventConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles incoming AmlAlertCreatedEvent.
    /// Logs for compliance audit trail and future escalation workflows.
    /// </summary>
    public async Task Consume(ConsumeContext<AmlAlertCreatedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "AmlAlertCreatedEvent received AlertId={AlertId} TransactionId={TransactionId} Severity={Severity} Type={AlertType} CorrelationId={CorrelationId}",
            evt.AlertId, evt.TransactionId, evt.Severity, evt.AlertType, evt.CorrelationId);

        try
        {
            // Log to compliance audit trail
            // In production, this could trigger:
            // - Escalation workflows for Critical alerts
            // - Regulatory reporting (e.g., COAF in Brazil)
            // - Customer notification procedures
            // - Fraud investigation systems

            _logger.LogInformation(
                "AmlAlertCreatedEvent logged to audit trail AlertId={AlertId}",
                evt.AlertId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AmlAlertCreatedEvent AlertId={AlertId}", evt.AlertId);
            throw;
        }
    }
}

