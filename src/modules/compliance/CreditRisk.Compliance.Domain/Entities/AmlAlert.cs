// File: src/modules/compliance/CreditRisk.Compliance.Domain/Entities/AmlAlert.cs
using CreditRisk.Compliance.Domain.Enums;
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.Compliance.Domain.Entities;

public sealed class AmlAlert : AggregateRoot
{
    public Guid TransactionId { get; private init; }
    public Guid CustomerId { get; private init; }
    public string AlertType { get; private init; } = string.Empty;
    public AlertSeverity Severity { get; private init; }
    public decimal TransactionAmount { get; private init; }
    public AlertStatus Status { get; private set; }
    public string? ReviewedBy { get; private set; }

    private AmlAlert() : base() { }

    public static AmlAlert Create(
        Guid transactionId,
        Guid customerId,
        string alertType,
        AlertSeverity severity,
        decimal transactionAmount)
    {
        Guard.AgainstEmpty(transactionId, nameof(transactionId));
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstNullOrWhiteSpace(alertType, nameof(alertType));

        return new AmlAlert
        {
            TransactionId = transactionId,
            CustomerId = customerId,
            AlertType = alertType,
            Severity = severity,
            TransactionAmount = transactionAmount,
            Status = AlertStatus.Open
        };
    }

    public void Review(AlertStatus newStatus, string reviewedBy)
    {
        Status = newStatus;
        ReviewedBy = reviewedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
