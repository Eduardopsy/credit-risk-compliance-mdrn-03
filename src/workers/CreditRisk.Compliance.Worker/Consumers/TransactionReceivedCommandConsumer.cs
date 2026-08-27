// File: src/workers/CreditRisk.Compliance.Worker/Consumers/TransactionReceivedCommandConsumer.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Compliance.Domain.Services;
using CreditRisk.Compliance.Infrastructure.Persistence;
using CreditRisk.Shared.Contracts.Compliance.Commands;
using CreditRisk.Shared.Contracts.Compliance.Events;
using CreditRisk.Shared.Kernel.Outbox;
using CreditRisk.Shared.Kernel.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditRisk.Compliance.Worker.Consumers;

/// <summary>
/// Consumes TransactionReceivedCommand and orchestrates compliance screening workflow.
/// 1. Persists transaction to database
/// 2. Screens customer against PEP database
/// 3. Evaluates AML rules
/// 4. Creates alerts if needed and publishes via Outbox
/// </summary>
public sealed class TransactionReceivedCommandConsumer : IConsumer<TransactionReceivedCommand>
{
    private readonly ILogger<TransactionReceivedCommandConsumer> _logger;
    private readonly IPepScreeningService _pepScreeningService;
    private readonly IAmlRulesEngine _amlRulesEngine;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAmlAlertRepository _amlAlertRepository;
    private readonly ComplianceDbContext _dbContext;
    private readonly IOutboxRepository _outboxRepository;

    public TransactionReceivedCommandConsumer(
        ILogger<TransactionReceivedCommandConsumer> logger,
        IPepScreeningService pepScreeningService,
        IAmlRulesEngine amlRulesEngine,
        ITransactionRepository transactionRepository,
        IAmlAlertRepository amlAlertRepository,
        ComplianceDbContext dbContext,
        IOutboxRepository outboxRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pepScreeningService = pepScreeningService ?? throw new ArgumentNullException(nameof(pepScreeningService));
        _amlRulesEngine = amlRulesEngine ?? throw new ArgumentNullException(nameof(amlRulesEngine));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _amlAlertRepository = amlAlertRepository ?? throw new ArgumentNullException(nameof(amlAlertRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
    }

    /// <summary>
    /// Handles incoming TransactionReceivedCommand.
    /// Workflow:
    /// 1. Persist transaction
    /// 2. Screen against PEP
    /// 3. Evaluate AML rules
    /// 4. Create alerts and publish via Outbox if triggered
    /// </summary>
    public async Task Consume(ConsumeContext<TransactionReceivedCommand> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "Processing TransactionReceivedCommand TransactionId={TransactionId} CustomerId={CustomerId} Amount={Amount} CorrelationId={CorrelationId}",
            cmd.TransactionId, cmd.CustomerId, cmd.Amount, cmd.CorrelationId);

        try
        {
            // Step 1: Persist transaction
            var transaction = Transaction.Create(
                customerId: cmd.CustomerId,
                amount: MoneyAmount.Create(cmd.Amount),
                transactionType: cmd.TransactionType,
                channel: cmd.Channel,
                transactionDate: cmd.TransactionDate,
                originAccountId: string.Empty,
                destinationAccountId: string.Empty);

            await _transactionRepository.AddAsync(transaction);
            _logger.LogInformation(
                "Transaction persisted TransactionId={TransactionId}",
                cmd.TransactionId);

            // Step 2: Screen against PEP (Politically Exposed Person)
            bool isPep = await _pepScreeningService.IsPersonPoliticallyExposedAsync(
                cmd.CustomerDocument,
                cmd.Channel);

            if (isPep)
            {
                _logger.LogWarning(
                    "Customer flagged as PEP TransactionId={TransactionId} CustomerId={CustomerId}",
                    cmd.TransactionId, cmd.CustomerId);

                // Create PEP alert
                await CreateAndPublishAlertAsync(
                    transactionId: transaction.Id,
                    customerId: cmd.CustomerId,
                    alertType: "PepMatch",
                    severity: "High",
                    transactionAmount: cmd.Amount,
                    correlationId: cmd.CorrelationId);

                transaction.Flag();
                await _transactionRepository.UpdateAsync(transaction);
            }

            // Step 3: Evaluate AML rules
            var amlAlert = await _amlRulesEngine.EvaluateTransactionAsync(
                customerId: cmd.CustomerId,
                amount: cmd.Amount,
                transactionType: cmd.TransactionType);

            if (amlAlert != null)
            {
                _logger.LogWarning(
                    "AML rule triggered TransactionId={TransactionId} RuleType={RuleType} Severity={Severity}",
                    cmd.TransactionId, amlAlert.RuleType, amlAlert.Severity);

                // Create AML rule alert
                await CreateAndPublishAlertAsync(
                    transactionId: transaction.Id,
                    customerId: cmd.CustomerId,
                    alertType: amlAlert.RuleType,
                    severity: amlAlert.Severity.ToString(),
                    transactionAmount: cmd.Amount,
                    correlationId: cmd.CorrelationId);

                if (amlAlert.Severity >= Domain.Enums.AlertSeverity.High)
                {
                    transaction.Flag();
                    await _transactionRepository.UpdateAsync(transaction);
                }
            }

            _logger.LogInformation(
                "TransactionReceivedCommand processed successfully TransactionId={TransactionId}",
                cmd.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TransactionReceivedCommand TransactionId={TransactionId}", cmd.TransactionId);
            throw;
        }
    }

    /// <summary>
    /// Creates an AML alert and publishes via Outbox for guaranteed delivery.
    /// </summary>
    private async Task CreateAndPublishAlertAsync(
        Guid transactionId,
        Guid customerId,
        string alertType,
        string severity,
        decimal transactionAmount,
        Guid correlationId)
    {
        // Create alert entity
        var alert = AmlAlert.Create(
            transactionId: transactionId,
            customerId: customerId,
            alertType: alertType,
            severity: Enum.Parse<Domain.Enums.AlertSeverity>(severity),
            transactionAmount: transactionAmount);

        await _amlAlertRepository.AddAsync(alert);

        // Publish event via Outbox for guaranteed delivery
        var amlAlertCreatedEvent = new AmlAlertCreatedEvent
        {
            AlertId = alert.Id,
            TransactionId = transactionId,
            CustomerId = customerId,
            AlertType = alertType,
            Severity = severity,
            TransactionAmount = transactionAmount,
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = correlationId
        };

        var outboxMessage = OutboxMessage.Create(
            messageType: typeof(AmlAlertCreatedEvent).FullName!,
            payload: JsonSerializer.Serialize(amlAlertCreatedEvent));

        await _outboxRepository.AddAsync(outboxMessage);

        _logger.LogInformation(
            "AML alert created and queued for publication AlertId={AlertId} OutboxMessageId={OutboxMessageId}",
            alert.Id, outboxMessage.Id);
    }
}
