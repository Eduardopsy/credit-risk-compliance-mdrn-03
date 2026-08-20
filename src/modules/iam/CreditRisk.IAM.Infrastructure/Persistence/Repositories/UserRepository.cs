// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Persistence/Repositories/UserRepository.cs
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.IAM.Domain.Repositories;
using CreditRisk.IAM.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CreditRisk.IAM.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IamDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
