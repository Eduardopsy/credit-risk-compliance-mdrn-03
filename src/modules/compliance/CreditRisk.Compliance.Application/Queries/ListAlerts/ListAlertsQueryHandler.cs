// File: src/modules/compliance/CreditRisk.Compliance.Application/Queries/ListAlerts/ListAlertsQueryHandler.cs
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Compliance.Domain.Enums;
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.Compliance.Application.Queries.ListAlerts;

public sealed class ListAlertsQueryHandler(IAmlAlertRepository alertRepository) : IQueryHandler<ListAlertsQuery, PagedResult<AmlAlertDto>>
{
    public async Task<Result<PagedResult<AmlAlertDto>>> HandleAsync(ListAlertsQuery query, CancellationToken cancellationToken = default)
    {
        var items = await alertRepository.GetByStatusAsync(AlertStatus.Open, query.Page, query.PageSize, cancellationToken).ConfigureAwait(false);
        var dtos = items.Select(a => new AmlAlertDto
        {
            Id = a.Id,
            TransactionId = a.TransactionId,
            CustomerId = a.CustomerId,
            AlertType = a.AlertType,
            Severity = a.Severity.ToString(),
            TransactionAmount = a.TransactionAmount,
            Status = a.Status.ToString(),
            ReviewedBy = a.ReviewedBy,
            CreatedAt = a.CreatedAt
        }).ToList();

        var paged = new PagedResult<AmlAlertDto>
        {
            Items = dtos,
            TotalCount = dtos.Count,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<PagedResult<AmlAlertDto>>.Success(paged);
    }
}
