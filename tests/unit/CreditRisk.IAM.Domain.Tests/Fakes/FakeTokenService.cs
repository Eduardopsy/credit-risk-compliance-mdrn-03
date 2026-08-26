// File: tests/unit/CreditRisk.IAM.Domain.Tests/Fakes/FakeTokenService.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Application.Ports;
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Domain.Tests.Fakes;

/// <summary>
/// Fake implementation of ITokenService for unit tests.
/// Supports configuring token responses for testing token generation flows.
/// </summary>
internal sealed class FakeTokenService : ITokenService
{
    private LoginResponse? _tokenResponse;
    private Exception? _generateThrow;
    private Exception? _refreshThrow;

    public Task<Result<LoginResponse>> GenerateTokensAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_generateThrow is not null)
            throw _generateThrow;

        if (_tokenResponse is null)
        {
            return Task.FromResult(Result<LoginResponse>.Failure(
                Error.Unauthorized("Token.GenerationFailed", "Token generation failed")));
        }

        return Task.FromResult(Result<LoginResponse>.Success(_tokenResponse));
    }

    public Task<Result<LoginResponse>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_refreshThrow is not null)
            throw _refreshThrow;

        if (_tokenResponse is null)
        {
            return Task.FromResult(Result<LoginResponse>.Failure(
                Error.Unauthorized("Token.RefreshFailed", "Token refresh failed")));
        }

        return Task.FromResult(Result<LoginResponse>.Success(_tokenResponse));
    }

    // Test helpers
    public void SetTokenResponse(LoginResponse response) => _tokenResponse = response;

    public void SetGenerateThrow(Exception exception) => _generateThrow = exception;

    public void SetRefreshThrow(Exception exception) => _refreshThrow = exception;

    public void Reset()
    {
        _tokenResponse = null;
        _generateThrow = null;
        _refreshThrow = null;
    }
}
