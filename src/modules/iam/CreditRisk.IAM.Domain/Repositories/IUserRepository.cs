// File: src/modules/iam/CreditRisk.IAM.Domain/Repositories/IUserRepository.cs
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.IAM.Domain.ValueObjects;

namespace CreditRisk.IAM.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
