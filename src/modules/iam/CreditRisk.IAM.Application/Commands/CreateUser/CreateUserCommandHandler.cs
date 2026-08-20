// File: src/modules/iam/CreditRisk.IAM.Application/Commands/CreateUser/CreateUserCommandHandler.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.IAM.Domain.Enums;
using CreditRisk.IAM.Domain.Repositories;
using CreditRisk.IAM.Domain.ValueObjects;
using CreditRisk.IAM.Application.Ports;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;
using MassTransit;

namespace CreditRisk.IAM.Application.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IPublishEndpoint publishEndpoint) : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);
        var existing = await userRepository.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<UserDto>.Failure(Error.Conflict("User.AlreadyExists", "A user with this email already exists."));
        }

        if (!Enum.TryParse<UserRole>(command.Role, true, out var role))
        {
            role = command.Role switch
            {
                "desk-operator" => UserRole.DeskOperator,
                "compliance-analyst" => UserRole.ComplianceAnalyst,
                "administrator" => UserRole.Administrator,
                _ => UserRole.DeskOperator
            };
        }

        var passwordHash = HashedPassword.Create(passwordHasher.Hash(command.TemporaryPassword));
        var user = User.Create(email, command.FullName, passwordHash, role, command.CreatedBy, command.CorrelationId);

        await userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);

        foreach (var domainEvent in user.DomainEvents)
        {
            if (domainEvent is CreditRisk.IAM.Domain.Events.UserCreatedDomainEvent ev)
            {
                await publishEndpoint.Publish<CreditRisk.Shared.Contracts.IAM.Events.UserCreatedEvent>(new
                {
                    ev.UserId,
                    ev.Email,
                    ev.Role,
                    CreatedAt = ev.OccurredAt,
                    ev.CreatedBy,
                    ev.CorrelationId
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            Role = user.Role.ToString().ToLowerInvariant(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }
}
