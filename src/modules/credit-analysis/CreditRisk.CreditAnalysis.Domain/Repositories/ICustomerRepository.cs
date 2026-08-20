// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Repositories/ICustomerRepository.cs
using CreditRisk.CreditAnalysis.Domain.Entities;

namespace CreditRisk.CreditAnalysis.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}
