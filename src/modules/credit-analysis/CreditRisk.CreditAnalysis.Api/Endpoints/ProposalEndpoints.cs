// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Endpoints/ProposalEndpoints.cs
using CreditRisk.CreditAnalysis.Application.Commands.CreateProposal;
using CreditRisk.CreditAnalysis.Application.Commands.SubmitProposal;
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Application.Queries.GetProposalById;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.CreditAnalysis.Api.Endpoints;

public static class ProposalEndpoints
{
    public static IEndpointRouteBuilder MapProposalEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/proposals")
            .WithTags("Credit Proposals")
            .RequireAuthorization("RequiresDeskOperator");

        group.MapPost("/", async (
            [FromBody] CreateProposalRequest request,
            CreateProposalCommandHandler handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            string operatorId = context.User.FindFirst("sub")?.Value ?? "operator";
            var command = new CreateProposalCommand(
                request.CustomerDocument,
                request.CustomerDocumentType,
                request.CustomerName,
                request.CustomerEmail,
                request.MonthlyIncome,
                request.RequestedLimit,
                request.ProposalType,
                request.BureauConsentGiven,
                request.BureauConsentIpAddress,
                operatorId,
                Guid.NewGuid());

            var result = await handler.HandleAsync(command, ct).ConfigureAwait(false);

            return result.IsSuccess
                .Equals(true)
                ? Results.Accepted($"/api/v1/proposals/{result.Value.Id}", new ProposalAcceptedResponse { ProposalId = result.Value.Id })
                : Results.UnprocessableEntity(result.Error);
        })
        .WithName("CreateProposal")
        .Produces<ProposalAcceptedResponse>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetProposalByIdQueryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetProposalByIdQuery(id), ct).ConfigureAwait(false);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound();
        })
        .WithName("GetProposalById")
        .Produces<CreditProposalDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/submit", async (
            Guid id,
            SubmitProposalCommandHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new SubmitProposalCommand(id), ct).ConfigureAwait(false);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound();
        })
        .WithName("SubmitProposal")
        .Produces<CreditProposalDto>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
