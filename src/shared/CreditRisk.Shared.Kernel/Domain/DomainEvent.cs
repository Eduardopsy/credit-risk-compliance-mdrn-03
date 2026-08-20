// File: src/shared/CreditRisk.Shared.Kernel/Domain/DomainEvent.cs
namespace CreditRisk.Shared.Kernel.Domain;

/// <summary>Base record for all domain events. Raised by aggregates, dispatched after UoW commits.</summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
