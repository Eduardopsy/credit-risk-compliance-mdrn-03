// File: src/modules/iam/CreditRisk.IAM.Application/Validators/LoginRequestValidator.cs
using CreditRisk.IAM.Application.DTOs;
using FluentValidation;

namespace CreditRisk.IAM.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.TotpCode).NotEmpty().Length(6);
    }
}
