// File: src/modules/compliance/CreditRisk.Compliance.Api/Endpoints/AlertEndpoints.cs
using CreditRisk.Compliance.Application.Commands.ReviewAlert;
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Compliance.Application.Queries.ListAlerts;
using CreditRisk.Shared.Kernel.Result;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.Compliance.Api.Endpoints;

public static class AlertEndpoints
{
    public static IEndpointRouteBuilder MapAlertEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/alerts")
            .WithTags("Compliance Alerts")
            .RequireAuthorization("RequiresComplianceAnalyst");

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ListAlertsQueryHandler handler,
            CancellationToken ct) =>
        {
            var query = new ListAlertsQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 10 : pageSize);
            var result = await handler.HandleAsync(query, ct).ConfigureAwait(false);
            return Results.Ok(result.Value);
        })
        .WithName("ListAlerts")
        .Produces<PagedResult<AmlAlertDto>>();

        group.MapPut("/{id:guid}/review", async (
            Guid id,
            [FromBody] ReviewAlertRequest request,
            ReviewAlertCommandHandler handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            string analystId = context.User.FindFirst("sub")?.Value ?? "analyst";
            var command = new ReviewAlertCommand(id, request.NewStatus, analystId);
            var result = await handler.HandleAsync(command, ct).ConfigureAwait(false);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound();
        })
        .WithName("ReviewAlert")
        .Produces<AmlAlertDto>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

public sealed record ReviewAlertRequest
{
    public required string NewStatus { get; init; }
}
