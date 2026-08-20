// File: src/modules/iam/CreditRisk.IAM.Domain/Entities/User.cs
using CreditRisk.IAM.Domain.Enums;
using CreditRisk.IAM.Domain.Events;
using CreditRisk.IAM.Domain.ValueObjects;
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.IAM.Domain.Entities;

/// <summary>Aggregate root representing a system user.</summary>
public sealed class User : AggregateRoot
{
    public Email Email { get; private set; } = null!;
    public string FullName { get; private set; } = string.Empty;
    public HashedPassword PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    private User() : base() { }

    public static User Create(Email email, string fullName, HashedPassword passwordHash, UserRole role, string createdBy, Guid correlationId)
    {
        Guard.AgainstNull(email, nameof(email));
        Guard.AgainstNullOrWhiteSpace(fullName, nameof(fullName));
        Guard.AgainstNull(passwordHash, nameof(passwordHash));
        Guard.AgainstNullOrWhiteSpace(createdBy, nameof(createdBy));

        var user = new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true
        };

        user.RaiseDomainEvent(new UserCreatedDomainEvent
        {
            UserId = user.Id,
            Email = email.Value,
            Role = role.ToString(),
            CreatedBy = createdBy,
            CorrelationId = correlationId
        });

        return user;
    }

    public void ChangeRole(UserRole newRole, string changedBy)
    {
        Role = newRole;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
