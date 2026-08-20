// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Services/KeycloakTokenService.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Application.Ports;
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Infrastructure.Services;

public sealed class KeycloakTokenService : ITokenService
{
    public Task<Result<LoginResponse>> GenerateTokensAsync(User user, CancellationToken cancellationToken = default)
    {
        var response = new LoginResponse
        {
            AccessToken = "mock-jwt-token-" + user.Id,
            RefreshToken = "mock-refresh-token-" + Guid.NewGuid(),
            ExpiresIn = 900,
            TokenType = "Bearer",
            Roles = [user.Role.ToString().ToLowerInvariant()]
        };
        return Task.FromResult(Result<LoginResponse>.Success(response));
    }

    public Task<Result<LoginResponse>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var response = new LoginResponse
        {
            AccessToken = "mock-refreshed-jwt-token",
            RefreshToken = "mock-refresh-token-" + Guid.NewGuid(),
            ExpiresIn = 900,
            TokenType = "Bearer",
            Roles = ["desk-operator"]
        };
        return Task.FromResult(Result<LoginResponse>.Success(response));
    }
}
