// File: tests/unit/CreditRisk.Compliance.Domain.Tests/Builders/TransactionBuilder.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Shared.Kernel.ValueObjects;

namespace CreditRisk.Compliance.Domain.Tests.Builders;

/// <summary>
/// Builder for Transaction test objects.
/// Produces a valid clean transaction (5000 amount, Online channel) by default.
/// </summary>
internal sealed class TransactionBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private decimal _amount = 5_000m;
    private string _transactionType = "Transfer";
    private string _channel = "Online";
    private DateTimeOffset _transactionDate = DateTimeOffset.UtcNow;
    private string _originAccountId = "origin-account-123";
    private string _destinationAccountId = "destination-account-456";

    public TransactionBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public TransactionBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public TransactionBuilder WithTransactionType(string transactionType)
    {
        _transactionType = transactionType;
        return this;
    }

    public TransactionBuilder WithChannel(string channel)
    {
        _channel = channel;
        return this;
    }

    public TransactionBuilder WithDate(DateTimeOffset date)
    {
        _transactionDate = date;
        return this;
    }

    public TransactionBuilder WithOriginAccountId(string accountId)
    {
        _originAccountId = accountId;
        return this;
    }

    public TransactionBuilder WithDestinationAccountId(string accountId)
    {
        _destinationAccountId = accountId;
        return this;
    }

    public Transaction Build()
    {
        return Transaction.Create(
            _customerId,
            MoneyAmount.Create(_amount),
            _transactionType,
            _channel,
            _transactionDate,
            _originAccountId,
            _destinationAccountId);
    }
}
