// File: src/modules/compliance/CreditRisk.Compliance.Domain/Repositories/IAmlAlertRepository.cs
using CreditRisk.Compliance.Domain.Entities;
using CreditRisk.Compliance.Domain.Enums;

namespace CreditRisk.Compliance.Domain.Repositories;

public interface IAmlAlertRepository
{
    Task<AmlAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AmlAlert>> GetByStatusAsync(AlertStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(AmlAlert alert, CancellationToken cancellationToken = default);
    Task UpdateAsync(AmlAlert alert, CancellationToken cancellationToken = default);
}
