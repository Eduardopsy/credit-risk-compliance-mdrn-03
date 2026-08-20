// File: src/modules/iam/CreditRisk.IAM.Domain/Events/UserCreatedDomainEvent.cs
using CreditRisk.Shared.Kernel.Domain;

namespace CreditRisk.IAM.Domain.Events;

public sealed record UserCreatedDomainEvent : DomainEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public required string CreatedBy { get; init; }
}
