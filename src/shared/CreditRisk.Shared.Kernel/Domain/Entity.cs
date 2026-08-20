// File: src/shared/CreditRisk.Shared.Kernel/Domain/Entity.cs
namespace CreditRisk.Shared.Kernel.Domain;

/// <summary>Base class for all domain entities. Identity equality semantics.</summary>
public abstract class Entity
{
    public Guid Id { get; protected init; }
    public DateTimeOffset CreatedAt { get; protected init; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected Entity(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Guard.Guard.AgainstEmpty(id, nameof(id));
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Entity? left, Entity? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
