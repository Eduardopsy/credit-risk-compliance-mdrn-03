// File: src/servers/CreditRisk.Operations.Server/Consumers/AmlAlertCreatedEventConsumer.cs
using CreditRisk.Operations.Server.Hubs;
using CreditRisk.Shared.Contracts.Compliance.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace CreditRisk.Operations.Server.Consumers;

/// <summary>
/// Consumes AmlAlertCreatedEvent and pushes it to compliance-analyst and administrator SignalR groups.
/// Queue: operations-hub_aml-alert-created-event
/// </summary>
public sealed class AmlAlertCreatedEventConsumer(
    IHubContext<OperationsHub> hubContext,
    ILogger<AmlAlertCreatedEventConsumer> logger)
    : IConsumer<AmlAlertCreatedEvent>
{
    public async Task Consume(ConsumeContext<AmlAlertCreatedEvent> context)
    {
        var message = context.Message;

        await hubContext.Clients
            .Group("role:compliance-analyst")
            .SendAsync("AmlAlertReceived", message, context.CancellationToken)
            .ConfigureAwait(false);

        await hubContext.Clients
            .Group("role:administrator")
            .SendAsync("AmlAlertReceived", message, context.CancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "AML alert {AlertId} pushed to compliance-analyst & administrator groups. Severity={Severity}",
            message.AlertId, message.Severity);
    }
}
