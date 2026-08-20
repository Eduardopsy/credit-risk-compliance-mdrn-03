// File: src/shared/CreditRisk.Shared.Contracts/IAM/Events/UserRoleChangedEvent.cs
namespace CreditRisk.Shared.Contracts.IAM.Events;

/// <summary>Published when a user's role is changed by an administrator.</summary>
public sealed record UserRoleChangedEvent
{
    public required Guid UserId { get; init; }
    public required string PreviousRole { get; init; }
    public required string NewRole { get; init; }
    public required string ChangedBy { get; init; }
    public required DateTimeOffset ChangedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
