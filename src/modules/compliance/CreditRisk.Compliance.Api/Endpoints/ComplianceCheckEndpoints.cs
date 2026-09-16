// File: src/modules/compliance/CreditRisk.Compliance.Api/Endpoints/ComplianceCheckEndpoints.cs
using System.Collections.Concurrent;
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Compliance.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.Compliance.Api.Endpoints;

public static class ComplianceCheckEndpoints
{
    private static readonly ConcurrentDictionary<Guid, ComplianceCheckResponse> ChecksStore = new();

    public static IEndpointRouteBuilder MapComplianceCheckEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/compliance/checks")
            .WithTags("Compliance Checks")
            .RequireAuthorization("RequiresComplianceAnalyst");

        group.MapPost("/", async (
            [FromBody] CreateComplianceCheckRequest request,
            IPepScreeningService pepService,
            CancellationToken ct) =>
        {
            bool isPep = await pepService.IsPersonPoliticallyExposedAsync(
                request.ApplicantDocument,
                request.ApplicantDocument.Length > 11 ? "CNPJ" : "CPF").ConfigureAwait(false);

            var checkId = Guid.NewGuid();
            var response = new ComplianceCheckResponse
            {
                Id = checkId,
                ProposalId = request.ProposalId,
                Status = isPep ? "flagged" : "clean",
                Checks = new ComplianceCheckDetails
                {
                    PepScreening = isPep ? "flagged" : "clean",
                    SanctionList = "clean",
                    AmlCheck = isPep ? "flagged" : "clean"
                },
                CreatedAt = DateTimeOffset.UtcNow
            };

            ChecksStore[checkId] = response;

            return Results.Created($"/api/v1/compliance/checks/{checkId}", response);
        })
        .WithName("CreateComplianceCheck")
        .Produces<ComplianceCheckResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/", () =>
        {
            var list = ChecksStore.Values.OrderByDescending(c => c.CreatedAt).ToList();
            return Results.Ok(list);
        })
        .WithName("ListComplianceChecks")
        .Produces<List<ComplianceCheckResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", (Guid id) =>
        {
            return ChecksStore.TryGetValue(id, out var check)
                ? Results.Ok(check)
                : Results.NotFound();
        })
        .WithName("GetComplianceCheckById")
        .Produces<ComplianceCheckResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
