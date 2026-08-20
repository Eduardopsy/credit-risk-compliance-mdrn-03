// File: src/modules/compliance/CreditRisk.Compliance.Application/Commands/IngestTransaction/IngestTransactionCommandHandler.cs
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;
using CreditRisk.Shared.Kernel.ValueObjects;
using MassTransit;

namespace CreditRisk.Compliance.Application.Commands.IngestTransaction;

public sealed class IngestTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IPublishEndpoint publishEndpoint) : ICommandHandler<IngestTransactionCommand, TransactionDto>
{
    public async Task<Result<TransactionDto>> HandleAsync(IngestTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var amount = MoneyAmount.Create(command.Amount);
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            amount,
            command.TransactionType,
            command.Channel,
            command.TransactionDate,
            command.OriginAccountId,
            command.DestinationAccountId);

        await transactionRepository.AddAsync(transaction, cancellationToken).ConfigureAwait(false);

        await publishEndpoint.Publish<CreditRisk.Shared.Contracts.Compliance.Commands.TransactionReceivedCommand>(new
        {
            TransactionId = transaction.Id,
            CustomerId = transaction.CustomerId,
            CustomerDocument = command.CustomerDocument,
            Amount = transaction.Amount.Amount,
            TransactionType = transaction.TransactionType,
            Channel = transaction.Channel,
            TransactionDate = transaction.TransactionDate,
            OriginAccountId = transaction.OriginAccountId,
            DestinationAccountId = transaction.DestinationAccountId,
            CorrelationId = command.CorrelationId
        }, cancellationToken).ConfigureAwait(false);

        return Result<TransactionDto>.Success(new TransactionDto
        {
            Id = transaction.Id,
            CustomerId = transaction.CustomerId,
            Amount = transaction.Amount.Amount,
            TransactionType = transaction.TransactionType,
            Channel = transaction.Channel,
            Status = transaction.Status.ToString(),
            TransactionDate = transaction.TransactionDate
        });
    }
}
