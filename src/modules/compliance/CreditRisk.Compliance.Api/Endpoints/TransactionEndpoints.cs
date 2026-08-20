// File: src/modules/compliance/CreditRisk.Compliance.Api/Endpoints/TransactionEndpoints.cs
using CreditRisk.Compliance.Application.Commands.IngestTransaction;
using CreditRisk.Compliance.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.Compliance.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/transactions")
            .WithTags("Transactions");

        group.MapPost("/", async (
            [FromBody] IngestTransactionRequest request,
            IngestTransactionCommandHandler handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            var command = new IngestTransactionCommand(
                request.TransactionId,
                request.CustomerDocument,
                request.CustomerDocumentType,
                request.Amount,
                request.TransactionType,
                request.Channel,
                request.TransactionDate,
                request.OriginAccountId,
                request.DestinationAccountId,
                Guid.Parse(context.TraceIdentifier.PadRight(36, '0')[..36]));

            var result = await handler.HandleAsync(command, ct).ConfigureAwait(false);

            return result.IsSuccess
                ? Results.Accepted($"/api/v1/transactions/{result.Value.Id}", result.Value)
                : Results.UnprocessableEntity(result.Error);
        })
        .WithName("IngestTransaction")
        .Produces<TransactionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
