// File: tests/unit/CreditRisk.Compliance.Domain.Tests/Builders/AmlAlertBuilder.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Enums;

namespace CreditRisk.Compliance.Domain.Tests.Builders;

/// <summary>
/// Builder for AmlAlert test objects.
/// Produces a valid alert with High severity by default.
/// </summary>
internal sealed class AmlAlertBuilder
{
    private Guid _transactionId = Guid.NewGuid();
    private Guid _customerId = Guid.NewGuid();
    private string _alertType = "Smurfing";
    private AlertSeverity _severity = AlertSeverity.High;
    private decimal _transactionAmount = 9_500m;

    public AmlAlertBuilder WithTransactionId(Guid transactionId)
    {
        _transactionId = transactionId;
        return this;
    }

    public AmlAlertBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public AmlAlertBuilder WithAlertType(string alertType)
    {
        _alertType = alertType;
        return this;
    }

    public AmlAlertBuilder WithSeverity(AlertSeverity severity)
    {
        _severity = severity;
        return this;
    }

    public AmlAlertBuilder WithTransactionAmount(decimal amount)
    {
        _transactionAmount = amount;
        return this;
    }

    public AmlAlert Build()
    {
        return AmlAlert.Create(
            _transactionId,
            _customerId,
            _alertType,
            _severity,
            _transactionAmount);
    }
}
