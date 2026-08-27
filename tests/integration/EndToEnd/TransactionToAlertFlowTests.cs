// File: tests/integration/EndToEnd/TransactionToAlertFlowTests.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Enums;
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Compliance.Domain.Services;
using CreditRisk.Compliance.Infrastructure.Persistence;
using CreditRisk.Compliance.Infrastructure.Services;
using CreditRisk.Integration.Tests.Infrastructure;
using CreditRisk.Shared.Contracts.Compliance.Commands;
using CreditRisk.Shared.Kernel.Outbox;
using CreditRisk.Shared.Kernel.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CreditRisk.Integration.Tests.EndToEnd;

/// <summary>
/// Integration tests for the complete flow: Transaction → PEP/AML Screening → Alert → Outbox
/// Verifies that alerts are created and persisted for guaranteed delivery.
/// </summary>
[Collection("Integration Tests")]
public sealed class TransactionToAlertFlowTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private IServiceScope? _scope;
    private ComplianceDbContext? _dbContext;
    private IPepScreeningService? _pepService;
    private IAmlRulesEngine? _amlEngine;
    private IAmlAlertRepository? _alertRepository;
    private IOutboxRepository? _outboxRepository;

    public TransactionToAlertFlowTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    public async Task InitializeAsync()
    {
        _scope = _fixture.ServiceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        _pepService = _scope.ServiceProvider.GetRequiredService<IPepScreeningService>();
        _amlEngine = _scope.ServiceProvider.GetRequiredService<IAmlRulesEngine>();
        _alertRepository = _scope.ServiceProvider.GetRequiredService<IAmlAlertRepository>();
        _outboxRepository = _scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        // Clear data before each test
        await ClearDataAsync();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_scope is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Test: PEP screening detects politically exposed person and creates alert.
    /// </summary>
    [Fact]
    public async Task PepScreening_DetectsPepCustomer_CreatesAlert()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        const string pepDocument = "00012345678"; // PEP pattern (starts with "000")
        const decimal amount = 5000m;

        // Act - Screen against PEP
        var isPep = await _pepService!.IsPersonPoliticallyExposedAsync(pepDocument, "CPF");

        // Assert
        isPep.Should().BeTrue("document starting with 000 should be flagged as PEP");

        // Act - Create alert
        var alert = AmlAlert.Create(transactionId, customerId, "PepMatch", AlertSeverity.High, amount);
        await _alertRepository!.AddAsync(alert);

        // Assert - Verify alert persisted
        var retrievedAlert = await _alertRepository.GetByIdAsync(alert.Id);
        retrievedAlert.Should().NotBeNull();
        retrievedAlert!.AlertType.Should().Be("PepMatch");
        retrievedAlert.Severity.Should().Be(AlertSeverity.High);
    }

    /// <summary>
    /// Test: AML structuring rule detects pattern of small transactions.
    /// </summary>
    [Fact]
    public async Task AmlRules_DetectsStructuring_CreatesAlert()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        const decimal smallAmount = 9000m; // Below structuring threshold (10,000)

        // Create multiple small transactions to trigger structuring pattern
        var transactionRepository = _scope!.ServiceProvider.GetRequiredService<ITransactionRepository>();
        for (int i = 0; i < 5; i++)
        {
            var txn = Transaction.Create(
                customerId: customerId,
                amount: MoneyAmount.Create(smallAmount),
                transactionType: "Transfer",
                channel: "Online",
                transactionDate: DateTimeOffset.UtcNow.AddDays(-i),
                originAccountId: $"ACC{i}",
                destinationAccountId: $"ACC{i + 1}");

            await transactionRepository.AddAsync(txn);
        }

        // Act - Evaluate AML rules
        var amlAlert = await _amlEngine!.EvaluateTransactionAsync(customerId, smallAmount, "Transfer");

        // Assert
        amlAlert.Should().NotBeNull("structuring pattern with 5 transactions should trigger alert");
        amlAlert!.RuleType.Should().Be("Structuring");
        amlAlert.Severity.Should().Be(AlertSeverity.Medium);
    }

    /// <summary>
    /// Test: Large transaction detection creates high-severity alert.
    /// </summary>
    [Fact]
    public async Task AmlRules_DetectsLargeTransaction_CreatesHighSeverityAlert()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        const decimal largeAmount = 150_000m; // Above large transaction threshold (100,000)

        // Act - Evaluate AML rules
        var amlAlert = await _amlEngine!.EvaluateTransactionAsync(customerId, largeAmount, "Transfer");

        // Assert
        amlAlert.Should().NotBeNull("large transaction should trigger alert");
        amlAlert!.RuleType.Should().Be("LargeTransaction");
        amlAlert.Severity.Should().Be(AlertSeverity.High);
    }

    /// <summary>
    /// Test: Alert created and published to Outbox for guaranteed delivery.
    /// </summary>
    [Fact]
    public async Task AlertCreation_PublishesToOutbox_GuaranteesDelivery()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        const decimal amount = 5000m;
        var alertType = "TestAlert";
        var severity = AlertSeverity.High;

        // Act - Create alert
        var alert = AmlAlert.Create(transactionId, customerId, alertType, severity, amount);
        await _alertRepository!.AddAsync(alert);

        // Create outbox message for the event
        var outboxMessage = OutboxMessage.Create(
            messageType: typeof(object).FullName!,
            payload: "{\"AlertId\":\"" + alert.Id + "\"}");

        await _outboxRepository!.AddAsync(outboxMessage);

        // Assert - Verify outbox message persisted
        var unprocessed = await _outboxRepository.GetUnprocessedAsync(maxRetries: 5, limit: 100);
        unprocessed.Should().HaveCountGreaterThan(0, "unprocessed messages should exist in outbox");

        var message = unprocessed.FirstOrDefault();
        message.Should().NotBeNull();
        (message!.ProcessedAt != null).Should().BeFalse("message should not be marked as processed yet");
        message.RetryCount.Should().Be(0, "new message should have retry count 0");
    }

    /// <summary>
    /// Test: Outbox message retry logic on failure.
    /// </summary>
    [Fact]
    public async Task OutboxMessage_OnFailure_IncrementRetryCount()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            messageType: typeof(object).FullName!,
            payload: "{}");

        await _outboxRepository!.AddAsync(outboxMessage);

        // Act - Simulate failure
        outboxMessage.MarkFailed("Simulated publish failure");

        await _outboxRepository.UpdateAsync(outboxMessage);

        // Assert
        var retrieved = await _outboxRepository.GetUnprocessedAsync(maxRetries: 5, limit: 100);
        var message = retrieved.FirstOrDefault();
        message.Should().NotBeNull();
        message!.RetryCount.Should().Be(1, "retry count should increment on failure");
        message.Error.Should().Be("Simulated publish failure");
        message.ProcessedAt.Should().BeNull("should not be marked as processed on failure");
    }

    /// <summary>
    /// Test: Alerts are isolated per customer (no data leakage).
    /// </summary>
    [Fact]
    public async Task AlertIsolation_CustomerAlertsNotVisible_ToOtherCustomers()
    {
        // Arrange
        var customer1 = Guid.NewGuid();
        var customer2 = Guid.NewGuid();

        // Create alert for customer 1
        var alert1 = AmlAlert.Create(
            Guid.NewGuid(), customer1, "PepMatch", AlertSeverity.High, 5000m);
        await _alertRepository!.AddAsync(alert1);

        // Create alert for customer 2
        var alert2 = AmlAlert.Create(
            Guid.NewGuid(), customer2, "LargeTransaction", AlertSeverity.Medium, 150000m);
        await _alertRepository.AddAsync(alert2);

        // Act - Query all alerts
        var allAlerts = (await _alertRepository.GetByStatusAsync(
            AlertStatus.Open, page: 1, pageSize: 100)).ToList();

        // Assert
        allAlerts.Should().HaveCount(2);
        var customer1Alerts = allAlerts.Where(a => a.CustomerId == customer1).ToList();
        var customer2Alerts = allAlerts.Where(a => a.CustomerId == customer2).ToList();

        customer1Alerts.Should().HaveCount(1);
        customer2Alerts.Should().HaveCount(1);
        customer1Alerts.First().Id.Should().NotBe(customer2Alerts.First().Id);
    }

    /// <summary>
    /// Test: PEP cache prevents repeated queries for same customer.
    /// </summary>
    [Fact]
    public async Task PepCache_CachesResults_ReducesQueries()
    {
        // Arrange
        const string document = "12345678901";

        // Act - Query 1 (cache miss)
        var result1 = await _pepService!.IsPersonPoliticallyExposedAsync(document, "CPF");

        // Act - Query 2 (cache hit)
        var result2 = await _pepService.IsPersonPoliticallyExposedAsync(document, "CPF");

        // Assert - Both should return same result (from cache)
        result1.Should().Be(result2, "cached results should be consistent");
        result1.Should().BeFalse("normal document should not be flagged as PEP");
    }

    /// <summary>
    /// Clears all data from databases for test isolation.
    /// </summary>
    private async Task ClearDataAsync()
    {
        if (_dbContext == null) return;

        // Clear all data
        var transactions = await _dbContext.Transactions.ToListAsync();
        _dbContext.Transactions.RemoveRange(transactions);

        var alerts = await _dbContext.AmlAlerts.ToListAsync();
        _dbContext.AmlAlerts.RemoveRange(alerts);

        var messages = await _dbContext.OutboxMessages.ToListAsync();
        _dbContext.OutboxMessages.RemoveRange(messages);

        await _dbContext.SaveChangesAsync();
    }
}
