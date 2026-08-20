// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Persistence/Repositories/AmlAlertRepository.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Enums;
using CreditRisk.Compliance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CreditRisk.Compliance.Infrastructure.Persistence.Repositories;

public sealed class AmlAlertRepository(ComplianceDbContext context) : IAmlAlertRepository
{
    public async Task<AmlAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.AmlAlerts.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AmlAlert>> GetByStatusAsync(AlertStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await context.AmlAlerts
            .Where(a => a.Status == status)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(AmlAlert alert, CancellationToken cancellationToken = default)
    {
        await context.AmlAlerts.AddAsync(alert, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(AmlAlert alert, CancellationToken cancellationToken = default)
    {
        context.AmlAlerts.Update(alert);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
