// File: tests/unit/CreditRisk.IAM.Domain.Tests/Handlers/LoginCommandHandlerTests.cs
using CreditRisk.IAM.Application.Commands.Login;
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Domain.Tests.Builders;
using CreditRisk.IAM.Domain.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace CreditRisk.IAM.Domain.Tests.Handlers;

public sealed class LoginCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeTokenService _tokenService = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _userRepository,
            _passwordHasher,
            _tokenService);
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("operator@example.com")
            .Build();

        _userRepository.Add(user);
        _passwordHasher.SetVerifyResult(true);
        _tokenService.SetTokenResponse(new LoginResponse
        {
            AccessToken = "access-token-123",
            RefreshToken = "refresh-token-456",
            ExpiresIn = 900,
            TokenType = "Bearer",
            Roles = ["DeskOperator"]
        });

        var command = new LoginCommand(
            Email: "operator@example.com",
            Password: "ValidPassword123!",
            TotpCode: "123456");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token-123");
        result.Value.ExpiresIn.Should().Be(900);
        result.Value.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsUnauthorizedError()
    {
        // Arrange — empty repository
        var command = new LoginCommand("nonexistent@example.com", "password", "123456");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.HttpStatusCode.Should().Be(401);
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsUnauthorizedError()
    {
        // Arrange
        var user = new UserBuilder().WithEmail("operator@example.com").Build();
        _userRepository.Add(user);
        _passwordHasher.SetVerifyResult(false); // Wrong password

        var command = new LoginCommand("operator@example.com", "WrongPassword", "123456");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.HttpStatusCode.Should().Be(401);
    }

    [Fact]
    public async Task HandleAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new LoginCommand("op@example.com", "pass", "123456");

        // Act
        Func<Task> act = () => _handler.HandleAsync(command, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
