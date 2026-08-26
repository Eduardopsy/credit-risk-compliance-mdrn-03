// File: tests/unit/CreditRisk.IAM.Domain.Tests/Builders/UserBuilder.cs
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.IAM.Domain.Enums;
using CreditRisk.IAM.Domain.ValueObjects;

namespace CreditRisk.IAM.Domain.Tests.Builders;

/// <summary>
/// Builder for User test objects.
/// Produces a valid active user with desk-operator role by default.
/// </summary>
internal sealed class UserBuilder
{
    private Email _email = Email.Create("test-user@example.com");
    private string _fullName = "Test User";
    private HashedPassword _passwordHash = HashedPassword.Create("hashed_password_123");
    private UserRole _role = UserRole.DeskOperator;
    private bool _isActive = true;
    private string _createdBy = "system-setup";
    private Guid _correlationId = Guid.NewGuid();

    public UserBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }

    public UserBuilder WithEmail(Email email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public UserBuilder WithPasswordHash(string hash)
    {
        _passwordHash = HashedPassword.Create(hash);
        return this;
    }

    public UserBuilder WithPasswordHash(HashedPassword passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithRole(UserRole role)
    {
        _role = role;
        return this;
    }

    public UserBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public UserBuilder WithCreatedBy(string createdBy)
    {
        _createdBy = createdBy;
        return this;
    }

    public UserBuilder WithCorrelationId(Guid correlationId)
    {
        _correlationId = correlationId;
        return this;
    }

    public User Build()
    {
        return User.Create(
            _email,
            _fullName,
            _passwordHash,
            _role,
            _createdBy,
            _correlationId);
    }
}
