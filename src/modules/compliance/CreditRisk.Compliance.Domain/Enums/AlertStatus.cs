// File: src/modules/compliance/CreditRisk.Compliance.Domain/Enums/AlertStatus.cs
namespace CreditRisk.Compliance.Domain.Enums;

public enum AlertStatus
{
    Open = 0,
    UnderReview = 1,
    Confirmed = 2,
    Dismissed = 3,
    Escalated = 4
}
