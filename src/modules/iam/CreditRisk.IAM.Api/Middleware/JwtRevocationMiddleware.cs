// File: src/modules/iam/CreditRisk.IAM.Api/Middleware/JwtRevocationMiddleware.cs
using CreditRisk.IAM.Application.Ports;

namespace CreditRisk.IAM.Api.Middleware;

public sealed class JwtRevocationMiddleware(
    RequestDelegate next,
    ITokenRevocationStore revocationStore,
    ILogger<JwtRevocationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string? jti = context.User.FindFirst("jti")?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                logger.LogWarning("Authenticated request missing jti claim. Rejecting.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            bool isRevoked = await revocationStore.IsRevokedAsync(jti, context.RequestAborted)
                .ConfigureAwait(false);

            if (isRevoked)
            {
                logger.LogWarning("Revoked token jti={Jti} rejected.", jti);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    new { error = "token_revoked", description = "The token has been revoked." },
                    context.RequestAborted)
                    .ConfigureAwait(false);
                return;
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
