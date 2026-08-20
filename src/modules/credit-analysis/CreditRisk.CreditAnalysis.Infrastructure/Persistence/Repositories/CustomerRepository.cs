// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/Repositories/CustomerRepository.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(CreditAnalysisDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Customers.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public async Task<Customer?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        return await context.Customers.FirstOrDefaultAsync(c => c.Document == document, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await context.Customers.AddAsync(customer, cancellationToken).ConfigureAwait(false);
    }
}
