// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Persistence/Repositories/TransactionRepository.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CreditRisk.Compliance.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(ComplianceDbContext context) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Transactions.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Transaction>> GetByCustomerIdAsync(Guid customerId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await context.Transactions
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await context.Transactions.AddAsync(transaction, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        context.Transactions.Update(transaction);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
