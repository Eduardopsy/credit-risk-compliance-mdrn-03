// File: src/modules/iam/CreditRisk.IAM.Application/Validators/CreateUserRequestValidator.cs
using CreditRisk.IAM.Application.DTOs;
using FluentValidation;

namespace CreditRisk.IAM.Application.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "desk-operator" or "compliance-analyst" or "administrator");
        RuleFor(x => x.TemporaryPassword).NotEmpty().MinimumLength(8);
    }
}
