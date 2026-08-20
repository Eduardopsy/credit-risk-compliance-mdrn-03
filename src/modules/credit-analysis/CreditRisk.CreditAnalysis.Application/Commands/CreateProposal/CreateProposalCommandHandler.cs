// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Commands/CreateProposal/CreateProposalCommandHandler.cs
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Application.Ports;
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;
using CreditRisk.Shared.Kernel.ValueObjects;
using MassTransit;

namespace CreditRisk.CreditAnalysis.Application.Commands.CreateProposal;

public sealed class CreateProposalCommandHandler(
    ICreditProposalRepository proposalRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : ICommandHandler<CreateProposalCommand, CreditProposalDto>
{
    public async Task<Result<CreditProposalDto>> HandleAsync(CreateProposalCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.BureauConsentGiven)
        {
            return Result<CreditProposalDto>.Failure(Error.Validation("Proposal.ConsentRequired", "Bureau consent is required to create a proposal."));
        }

        var customer = await customerRepository.GetByDocumentAsync(command.CustomerDocument, cancellationToken).ConfigureAwait(false);
        if (customer is null)
        {
            customer = Customer.Create(
                command.CustomerDocument,
                command.CustomerDocumentType,
                command.CustomerName,
                command.CustomerEmail,
                command.MonthlyIncome);
            await customerRepository.AddAsync(customer, cancellationToken).ConfigureAwait(false);
        }

        var requestedLimit = MoneyAmount.Create(command.RequestedLimit);
        var proposal = CreditProposal.Create(customer.Id, requestedLimit, command.ProposalType, command.CreatedBy, command.CorrelationId);

        await proposalRepository.AddAsync(proposal, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        foreach (var domainEvent in proposal.DomainEvents)
        {
            if (domainEvent is CreditRisk.CreditAnalysis.Domain.Events.CreditProposalCreatedDomainEvent ev)
            {
                await publishEndpoint.Publish<CreditRisk.Shared.Contracts.CreditAnalysis.Events.CreditProposalCreatedEvent>(new
                {
                    ev.ProposalId,
                    ev.CustomerId,
                    CustomerDocument = customer.Document,
                    CustomerDocumentType = customer.DocumentType,
                    ev.RequestedLimit,
                    ev.ProposalType,
                    CreatedAt = ev.OccurredAt,
                    CreatedBy = proposal.CreatedBy,
                    ev.CorrelationId
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        return Result<CreditProposalDto>.Success(new CreditProposalDto
        {
            Id = proposal.Id,
            CustomerId = proposal.CustomerId,
            CustomerDocument = customer.Document,
            RequestedLimit = proposal.RequestedLimit.Amount,
            ApprovedLimit = proposal.ApprovedLimit?.Amount,
            Status = proposal.Status.ToString(),
            RiskRating = proposal.Rating?.ToString(),
            RequiresManualReview = proposal.RequiresManualReview,
            CreatedAt = proposal.CreatedAt,
            UpdatedAt = proposal.UpdatedAt,
            CreatedBy = proposal.CreatedBy
        });
    }
}
