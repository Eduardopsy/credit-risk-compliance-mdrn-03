// File: src/servers/CreditRisk.Operations.Server/Consumers/CreditLimitApprovedEventConsumer.cs
using CreditRisk.Operations.Server.Hubs;
using CreditRisk.Shared.Contracts.CreditAnalysis.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace CreditRisk.Operations.Server.Consumers;

/// <summary>
/// Consumes CreditLimitApprovedEvent and pushes it to desk-operator and administrator SignalR groups.
/// </summary>
public sealed class CreditLimitApprovedEventConsumer(
    IHubContext<OperationsHub> hubContext,
    ILogger<CreditLimitApprovedEventConsumer> logger)
    : IConsumer<CreditLimitApprovedEvent>
{
    public async Task Consume(ConsumeContext<CreditLimitApprovedEvent> context)
    {
        var message = context.Message;

        await hubContext.Clients
            .Group("role:desk-operator")
            .SendAsync("CreditLimitApproved", message, context.CancellationToken)
            .ConfigureAwait(false);

        await hubContext.Clients
            .Group("role:administrator")
            .SendAsync("CreditLimitApproved", message, context.CancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "CreditLimitApproved {ProposalId} pushed to desk-operator & administrator groups. Limit={ApprovedLimit}",
            message.ProposalId, message.ApprovedLimit);
    }
}
