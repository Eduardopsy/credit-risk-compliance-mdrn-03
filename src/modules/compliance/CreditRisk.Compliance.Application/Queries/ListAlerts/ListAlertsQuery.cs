// File: src/modules/compliance/CreditRisk.Compliance.Application/Queries/ListAlerts/ListAlertsQuery.cs
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.Compliance.Application.Queries.ListAlerts;

public sealed record ListAlertsQuery(int Page, int PageSize);
