// File: src/shared/CreditRisk.Shared.Kernel/Domain/AggregateRoot.cs
namespace CreditRisk.Shared.Kernel.Domain;

/// <summary>Base class for aggregate roots. Extends Entity with domain event collection.</summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base() { }

    protected AggregateRoot(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt) { }

    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        Guard.Guard.AgainstNull(domainEvent, nameof(domainEvent));
        _domainEvents.Add(domainEvent);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
