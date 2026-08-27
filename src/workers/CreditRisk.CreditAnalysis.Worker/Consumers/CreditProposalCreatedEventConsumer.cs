// File: src/workers/CreditRisk.CreditAnalysis.Worker/Consumers/CreditProposalCreatedEventConsumer.cs
using CreditRisk.CreditAnalysis.Domain.Services;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence;
using CreditRisk.CreditAnalysis.Worker.Services;
using CreditRisk.Shared.Contracts.CreditAnalysis.Events;
using CreditRisk.Shared.Kernel.Outbox;
using CreditRisk.Shared.Kernel.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditRisk.CreditAnalysis.Worker.Consumers;

/// <summary>
/// Consumes CreditProposalCreatedEvent and orchestrates credit analysis workflow.
/// Fetches bureau data, scores the proposal using CreditScoringEngine, and publishes CreditProposalEvaluatedEvent via Outbox.
/// </summary>
public sealed class CreditProposalCreatedEventConsumer : IConsumer<CreditProposalCreatedEvent>
{
    private readonly ILogger<CreditProposalCreatedEventConsumer> _logger;
    private readonly ICreditScoringEngine _scoringEngine;
    private readonly BureauHttpClient _bureauClient;
    private readonly CreditAnalysisDbContext _dbContext;
    private readonly IOutboxRepository _outboxRepository;

    public CreditProposalCreatedEventConsumer(
        ILogger<CreditProposalCreatedEventConsumer> logger,
        ICreditScoringEngine scoringEngine,
        BureauHttpClient bureauClient,
        CreditAnalysisDbContext dbContext,
        IOutboxRepository outboxRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scoringEngine = scoringEngine ?? throw new ArgumentNullException(nameof(scoringEngine));
        _bureauClient = bureauClient ?? throw new ArgumentNullException(nameof(bureauClient));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
    }

    /// <summary>
    /// Handles incoming CreditProposalCreatedEvent.
    /// 1. Queries bureau for applicant data
    /// 2. Runs CreditScoringEngine with bureau score and income
    /// 3. Publishes CreditProposalEvaluatedEvent via Outbox (guaranteed delivery)
    /// 4. Gracefully degrades if bureau is unavailable (score=0, rating=E)
    /// </summary>
    public async Task Consume(ConsumeContext<CreditProposalCreatedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Processing CreditProposalCreatedEvent ProposalId={ProposalId} CustomerId={CustomerId} Document={Document} RequestedLimit={RequestedLimit} CorrelationId={CorrelationId}",
            evt.ProposalId, evt.CustomerId, evt.CustomerDocument, evt.RequestedLimit, evt.CorrelationId);

        try
        {
            // Step 1: Query bureau for applicant data (with graceful degradation)
            var bureauData = await _bureauClient.QueryAsync(evt.CustomerDocument, evt.CustomerDocumentType);
            
            // Step 2: Determine bureau score and extract income
            // If bureau is unavailable, default to score 0 (rating E - rejection)
            int bureauScore = bureauData?.Score ?? 0;
            decimal monthlyIncome = ExtractMonthlyIncomeFromProposal(evt);

            _logger.LogInformation(
                "Bureau query result ProposalId={ProposalId} BureauScore={BureauScore} MonthlyIncome={MonthlyIncome}",
                evt.ProposalId, bureauScore, monthlyIncome);

            // Step 3: Run scoring engine
            var requestedLimitMoney = MoneyAmount.Create(evt.RequestedLimit);
            var (riskRating, approvedLimit, requiresManualReview) = _scoringEngine.Evaluate(
                monthlyIncome,
                requestedLimitMoney,
                bureauScore);

            _logger.LogInformation(
                "Credit scoring completed ProposalId={ProposalId} RiskRating={RiskRating} ApprovedLimit={ApprovedLimit} RequiresManualReview={RequiresManualReview}",
                evt.ProposalId, riskRating, approvedLimit.Amount, requiresManualReview);

            // Step 4: Create and persist evaluation event via Outbox (guaranteed delivery)
            var evaluatedEvent = new CreditProposalEvaluatedEvent
            {
                ProposalId = evt.ProposalId,
                CustomerId = evt.CustomerId,
                RiskRating = riskRating.ToString(),
                ApprovedLimit = approvedLimit.Amount,
                EvaluatedAt = DateTimeOffset.UtcNow,
                EvaluatedBy = "AUTO",
                RequiresManualReview = requiresManualReview,
                CorrelationId = evt.CorrelationId
            };

            // Persist to Outbox for reliable publication
            var payload = JsonSerializer.Serialize(evaluatedEvent);
            var outboxMessage = OutboxMessage.Create(
                messageType: typeof(CreditProposalEvaluatedEvent).FullName!,
                payload: payload);

            await _outboxRepository.AddAsync(outboxMessage);

            _logger.LogInformation(
                "CreditProposalCreatedEvent processed and persisted to Outbox ProposalId={ProposalId} OutboxMessageId={OutboxMessageId}",
                evt.ProposalId, outboxMessage.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CreditProposalCreatedEvent ProposalId={ProposalId}", evt.ProposalId);
            throw;
        }
    }

    /// <summary>
    /// Extracts monthly income from proposal. In a real scenario, this would come from a separate service or database.
    /// For now, we use a placeholder: estimate based on requested limit (heuristic).
    /// </summary>
    private static decimal ExtractMonthlyIncomeFromProposal(CreditProposalCreatedEvent evt)
    {
        // Placeholder: Estimate monthly income as 5% of requested limit per month
        // In production, this would come from a customer profile or income verification service
        return evt.RequestedLimit * 0.05m;
    }
}

