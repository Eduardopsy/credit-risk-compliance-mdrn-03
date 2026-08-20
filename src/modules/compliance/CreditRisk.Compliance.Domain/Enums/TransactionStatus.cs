// File: src/modules/compliance/CreditRisk.Compliance.Domain/Enums/TransactionStatus.cs
namespace CreditRisk.Compliance.Domain.Enums;

public enum TransactionStatus
{
    Received = 0,
    Processed = 1,
    Flagged = 2,
    Blocked = 3
}
