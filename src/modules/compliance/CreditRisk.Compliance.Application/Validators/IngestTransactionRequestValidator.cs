// File: src/modules/compliance/CreditRisk.Compliance.Application/Validators/IngestTransactionRequestValidator.cs
using CreditRisk.Compliance.Application.DTOs;
using FluentValidation;

namespace CreditRisk.Compliance.Application.Validators;

public sealed class IngestTransactionRequestValidator : AbstractValidator<IngestTransactionRequest>
{
    public IngestTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.CustomerDocument).NotEmpty().MaximumLength(14);
        RuleFor(x => x.CustomerDocumentType).NotEmpty().Must(t => t is "CPF" or "CNPJ");
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.TransactionType).NotEmpty().Must(t => t is "Credit" or "Debit" or "Transfer");
        RuleFor(x => x.Channel).NotEmpty().Must(c => c is "ATM" or "Online" or "Branch" or "Mobile");
        RuleFor(x => x.OriginAccountId).NotEmpty();
        RuleFor(x => x.DestinationAccountId).NotEmpty();
    }
}
