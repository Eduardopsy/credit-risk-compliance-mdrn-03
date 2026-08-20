// File: src/modules/iam/CreditRisk.IAM.Api/Endpoints/AuthEndpoints.cs
using CreditRisk.IAM.Application.Commands.Login;
using CreditRisk.IAM.Application.Commands.Logout;
using CreditRisk.IAM.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.IAM.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            LoginCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new LoginCommand(request.Email, request.Password, request.TotpCode);
            var result = await handler.HandleAsync(command, ct).ConfigureAwait(false);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Unauthorized();
        })
        .WithName("Login")
        .Produces<LoginResponse>()
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", async (
            LogoutCommandHandler handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            string? jti = context.User.FindFirst("jti")?.Value ?? "mock-jti";
            var command = new LogoutCommand(jti, TimeSpan.FromMinutes(15));
            await handler.HandleAsync(command, ct).ConfigureAwait(false);
            return Results.NoContent();
        })
        .WithName("Logout")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
