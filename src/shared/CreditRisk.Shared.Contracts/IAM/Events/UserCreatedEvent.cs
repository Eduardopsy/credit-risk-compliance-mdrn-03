// File: src/shared/CreditRisk.Shared.Contracts/IAM/Events/UserCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.IAM.Events;

/// <summary>Published when a new user account is created.</summary>
public sealed record UserCreatedEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }               // "desk-operator" | "compliance-analyst" | "administrator"
    public required DateTimeOffset CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public required Guid CorrelationId { get; init; }
}
