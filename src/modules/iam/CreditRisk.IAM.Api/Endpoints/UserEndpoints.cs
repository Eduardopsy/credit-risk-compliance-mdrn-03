// File: src/modules/iam/CreditRisk.IAM.Api/Endpoints/UserEndpoints.cs
using CreditRisk.IAM.Application.Commands.CreateUser;
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Application.Queries.GetUserById;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.IAM.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/users")
            .WithTags("Users");

        group.MapPost("/", async (
            [FromBody] CreateUserRequest request,
            CreateUserCommandHandler handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            string adminId = context.User.FindFirst("sub")?.Value ?? "admin";
            var command = new CreateUserCommand(
                request.Email,
                request.FullName,
                request.Role,
                request.TemporaryPassword,
                adminId,
                Guid.NewGuid());

            var result = await handler.HandleAsync(command, ct).ConfigureAwait(false);

            return result.IsSuccess
                ? Results.Created($"/api/v1/users/{result.Value.Id}", result.Value)
                : Results.UnprocessableEntity(result.Error);
        })
        .WithName("CreateUser")
        .Produces<UserDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (
            Guid id,
            GetUserByIdQueryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetUserByIdQuery(id), ct).ConfigureAwait(false);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound();
        })
        .WithName("GetUserById")
        .RequireAuthorization("RequiresAdministrator")
        .Produces<UserDto>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
