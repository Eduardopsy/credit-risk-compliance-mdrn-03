// File: src/servers/CreditRisk.Operations.Server/Consumers/TransactionFlaggedEventConsumer.cs
using CreditRisk.Operations.Server.Hubs;
using CreditRisk.Shared.Contracts.Compliance.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace CreditRisk.Operations.Server.Consumers;

/// <summary>
/// Consumes TransactionFlaggedEvent and pushes it to compliance-analyst and administrator SignalR groups.
/// </summary>
public sealed class TransactionFlaggedEventConsumer(
    IHubContext<OperationsHub> hubContext,
    ILogger<TransactionFlaggedEventConsumer> logger)
    : IConsumer<TransactionFlaggedEvent>
{
    public async Task Consume(ConsumeContext<TransactionFlaggedEvent> context)
    {
        var message = context.Message;

        await hubContext.Clients
            .Group("role:compliance-analyst")
            .SendAsync("ReceiveTransactionFlagged", message, context.CancellationToken)
            .ConfigureAwait(false);

        await hubContext.Clients
            .Group("role:administrator")
            .SendAsync("ReceiveTransactionFlagged", message, context.CancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "TransactionFlagged {TransactionId} pushed to compliance-analyst & administrator groups. Reason={Reason}",
            message.TransactionId, message.FlagReason);
    }
}
