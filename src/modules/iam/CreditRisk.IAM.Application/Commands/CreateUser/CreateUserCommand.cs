// File: src/modules/iam/CreditRisk.IAM.Application/Commands/CreateUser/CreateUserCommand.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FullName,
    string Role,
    string TemporaryPassword,
    string CreatedBy,
    Guid CorrelationId);
