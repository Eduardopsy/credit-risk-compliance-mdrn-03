// File: tests/unit/CreditRisk.IAM.Domain.Tests/Handlers/LogoutCommandHandlerTests.cs
using CreditRisk.IAM.Application.Commands.Logout;
using CreditRisk.IAM.Domain.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace CreditRisk.IAM.Domain.Tests.Handlers;

public sealed class LogoutCommandHandlerTests
{
    private readonly FakeTokenRevocationStore _revocationStore = new();
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _handler = new LogoutCommandHandler(_revocationStore);
    }

    [Fact]
    public async Task HandleAsync_ValidJti_AddsJtiToRevocationStore()
    {
        // Arrange
        string jti = Guid.NewGuid().ToString();
        var command = new LogoutCommand(Jti: jti, ExpiresIn: TimeSpan.FromMinutes(10));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _revocationStore.Contains(jti).Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_EmptyJti_ReturnsValidationError()
    {
        // Arrange
        var command = new LogoutCommand(Jti: string.Empty, ExpiresIn: TimeSpan.FromMinutes(10));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.HttpStatusCode.Should().Be(422);
    }

    [Fact]
    public async Task HandleAsync_AlreadyRevokedJti_ReturnsSuccess()
    {
        // Arrange — idempotent revocation
        string jti = Guid.NewGuid().ToString();
        _revocationStore.PreloadRevoked(jti);
        var command = new LogoutCommand(Jti: jti, ExpiresIn: TimeSpan.FromMinutes(5));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert — revocation is idempotent
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new LogoutCommand(Jti: Guid.NewGuid().ToString(), ExpiresIn: TimeSpan.FromMinutes(10));

        // Act
        Func<Task> act = () => _handler.HandleAsync(command, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
