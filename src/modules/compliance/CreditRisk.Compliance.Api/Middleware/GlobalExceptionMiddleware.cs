// File: src/modules/compliance/CreditRisk.Compliance.Api/Middleware/GlobalExceptionMiddleware.cs
using CreditRisk.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.Compliance.Api.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain exception: {ErrorCode} — {Message}", ex.ErrorCode, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status422UnprocessableEntity,
                "Domain Rule Violation", ex.Message, ex.ErrorCode).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            string correlationId = context.TraceIdentifier;
            logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please contact support with the correlation ID.",
                "InternalError",
                correlationId).ConfigureAwait(false);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string errorCode,
        string? correlationId = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["errorCode"] = errorCode;
        if (correlationId is not null)
            problem.Extensions["correlationId"] = correlationId;

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted).ConfigureAwait(false);
    }
}
