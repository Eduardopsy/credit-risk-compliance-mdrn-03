// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Validators/CreateProposalRequestValidator.cs
using CreditRisk.CreditAnalysis.Application.DTOs;
using FluentValidation;

namespace CreditRisk.CreditAnalysis.Application.Validators;

public sealed class CreateProposalRequestValidator : AbstractValidator<CreateProposalRequest>
{
    public CreateProposalRequestValidator()
    {
        RuleFor(x => x.CustomerDocument).NotEmpty().MaximumLength(14);
        RuleFor(x => x.CustomerDocumentType).NotEmpty().Must(t => t is "CPF" or "CNPJ");
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.MonthlyIncome).GreaterThan(0);
        RuleFor(x => x.RequestedLimit).GreaterThan(0).LessThanOrEqualTo(500_000m);
        RuleFor(x => x.ProposalType).NotEmpty().Must(t => t is "Individual" or "LegalEntity");
        RuleFor(x => x.BureauConsentGiven).Equal(true);
    }
}
