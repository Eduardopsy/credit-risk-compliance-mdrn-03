// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Entities/Customer.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.CreditAnalysis.Domain.Entities;

public sealed class Customer : Entity
{
    public string Document { get; private init; } = string.Empty;
    public string DocumentType { get; private init; } = string.Empty;
    public string Name { get; private init; } = string.Empty;
    public string Email { get; private init; } = string.Empty;
    public decimal MonthlyIncome { get; private init; }

    private Customer() : base() { }

    public static Customer Create(string document, string documentType, string name, string email, decimal monthlyIncome)
    {
        Guard.AgainstNullOrWhiteSpace(document, nameof(document));
        Guard.AgainstNullOrWhiteSpace(documentType, nameof(documentType));
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNegative(monthlyIncome, nameof(monthlyIncome));

        return new Customer
        {
            Document = document,
            DocumentType = documentType,
            Name = name,
            Email = email,
            MonthlyIncome = monthlyIncome
        };
    }
}
