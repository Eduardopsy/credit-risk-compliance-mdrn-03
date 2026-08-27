// File: src/modules/compliance/CreditRisk.Compliance.Domain/Repositories/ITransactionRepository.cs
using CreditRisk.Compliance.Domain.Entities;

namespace CreditRisk.Compliance.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByCustomerIdAsync(Guid customerId, int limit = 50, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
