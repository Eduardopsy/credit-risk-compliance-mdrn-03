// File: src/modules/compliance/CreditRisk.Compliance.Domain/Entities/Transaction.cs
using CreditRisk.Compliance.Domain.Enums;
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Guard;
using CreditRisk.Shared.Kernel.ValueObjects;

namespace CreditRisk.Compliance.Domain.Entities;

public sealed class Transaction : AggregateRoot
{
    public Guid CustomerId { get; private init; }
    public MoneyAmount Amount { get; private init; } = MoneyAmount.Zero();
    public string TransactionType { get; private init; } = string.Empty;
    public string Channel { get; private init; } = string.Empty;
    public DateTimeOffset TransactionDate { get; private init; }
    public string OriginAccountId { get; private init; } = string.Empty;
    public string DestinationAccountId { get; private init; } = string.Empty;
    public TransactionStatus Status { get; private set; }

    private Transaction() : base() { }

    public static Transaction Create(
        Guid customerId,
        MoneyAmount amount,
        string transactionType,
        string channel,
        DateTimeOffset transactionDate,
        string originAccountId,
        string destinationAccountId)
    {
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstNull(amount, nameof(amount));
        Guard.AgainstNullOrWhiteSpace(transactionType, nameof(transactionType));
        Guard.AgainstNullOrWhiteSpace(channel, nameof(channel));

        return new Transaction
        {
            CustomerId = customerId,
            Amount = amount,
            TransactionType = transactionType,
            Channel = channel,
            TransactionDate = transactionDate,
            OriginAccountId = originAccountId,
            DestinationAccountId = destinationAccountId,
            Status = TransactionStatus.Received
        };
    }

    public void Flag()
    {
        Status = TransactionStatus.Flagged;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Block()
    {
        Status = TransactionStatus.Blocked;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
