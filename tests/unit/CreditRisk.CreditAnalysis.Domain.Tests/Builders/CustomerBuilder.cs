// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Builders/CustomerBuilder.cs
using CreditRisk.CreditAnalysis.Domain.Entities;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Builders;

/// <summary>
/// Builder for Customer test objects.
/// Produces a valid customer with CPF, name, and email by default.
/// </summary>
internal sealed class CustomerBuilder
{
    private string _document = "52998224725";  // Valid CPF format
    private string _documentType = "CPF";
    private string _name = "Test Customer";
    private string _email = "customer@example.com";
    private decimal _monthlyIncome = 5_000m;

    public CustomerBuilder WithDocument(string document)
    {
        _document = document;
        return this;
    }

    public CustomerBuilder WithDocumentType(string documentType)
    {
        _documentType = documentType;
        return this;
    }

    public CustomerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithMonthlyIncome(decimal income)
    {
        _monthlyIncome = income;
        return this;
    }

    public Customer Build()
    {
        return Customer.Create(
            _document,
            _documentType,
            _name,
            _email,
            _monthlyIncome);
    }
}
