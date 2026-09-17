// File: src/modules/operations/CreditRisk.Operations.Client/Services/ApiClient.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Models.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace CreditRisk.Operations.Client.Services;

/// <summary>
/// Typed HTTP client for all back-end API calls.
/// Injects JWT bearer token when available.
/// </summary>
public sealed class ApiClient(HttpClient httpClient, IAccessTokenProvider tokenProvider)
{
    private async Task EnsureAuthHeaderAsync()
    {
        try
        {
            var result = await tokenProvider.RequestAccessToken();
            if (result.TryGetToken(out var token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Value);
            }
        }
        catch
        {
            // Proceed without token if not logged in
        }
    }

    // ── Credit Proposals ────────────────────────────────────────────────────────

    public async Task<CreditProposalViewModel?> GetProposalAsync(
        Guid id, CancellationToken ct = default)
    {
        await EnsureAuthHeaderAsync();
        return await httpClient.GetFromJsonAsync<CreditProposalViewModel>(
            $"api/v1/proposals/{id}", ct).ConfigureAwait(false);
    }

    public async Task<PagedResult<CreditProposalViewModel>?> GetProposalsAsync(
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        await EnsureAuthHeaderAsync();
        return await httpClient.GetFromJsonAsync<PagedResult<CreditProposalViewModel>>(
            $"api/v1/proposals?page={page}&pageSize={pageSize}", ct).ConfigureAwait(false);
    }

    public async Task<ProposalAcceptedResponse?> CreateProposalAsync(
        CreateProposalRequest request, CancellationToken ct = default)
    {
        await EnsureAuthHeaderAsync();
        var response = await httpClient.PostAsJsonAsync("api/v1/proposals", request, ct)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProposalAcceptedResponse>(ct)
            .ConfigureAwait(false);
    }

    // ── AML Alerts ──────────────────────────────────────────────────────────────

    public async Task<PagedResult<AmlAlertViewModel>?> GetAlertsAsync(
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        await EnsureAuthHeaderAsync();
        return await httpClient.GetFromJsonAsync<PagedResult<AmlAlertViewModel>>(
            $"api/v1/alerts?page={page}&pageSize={pageSize}", ct).ConfigureAwait(false);
    }

    public async Task<AmlAlertViewModel?> GetAlertAsync(
        Guid id, CancellationToken ct = default)
    {
        await EnsureAuthHeaderAsync();
        return await httpClient.GetFromJsonAsync<AmlAlertViewModel>(
            $"api/v1/alerts/{id}", ct).ConfigureAwait(false);
    }

    public async Task<AmlAlertViewModel?> ReviewAlertAsync(
        Guid alertId, ReviewAlertRequest request, CancellationToken ct = default)
    {
        await EnsureAuthHeaderAsync();
        var response = await httpClient.PutAsJsonAsync(
            $"api/v1/alerts/{alertId}/review", request, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AmlAlertViewModel>(ct).ConfigureAwait(false);
    }

    // ── Metrics ─────────────────────────────────────────────────────────────────

    public async Task<DashboardMetricsViewModel?> GetDashboardMetricsAsync(
        CancellationToken ct = default)
    {
        try
        {
            await EnsureAuthHeaderAsync();
            return await httpClient.GetFromJsonAsync<DashboardMetricsViewModel>(
                "api/v1/metrics/risk-dashboard", ct).ConfigureAwait(false);
        }
        catch
        {
            return new DashboardMetricsViewModel();
        }
    }

    public async Task<OperationsMetricsViewModel?> GetOperationsMetricsAsync(
        CancellationToken ct = default)
    {
        try
        {
            await EnsureAuthHeaderAsync();
            return await httpClient.GetFromJsonAsync<OperationsMetricsViewModel>(
                "api/v1/metrics/operations-dashboard", ct).ConfigureAwait(false);
        }
        catch
        {
            return new OperationsMetricsViewModel();
        }
    }

    // ── Audit Logs ──────────────────────────────────────────────────────────────

    public async Task<PagedResult<AuditLogEntryViewModel>?> GetAuditLogsAsync(
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            await EnsureAuthHeaderAsync();
            return await httpClient.GetFromJsonAsync<PagedResult<AuditLogEntryViewModel>>(
                $"api/v1/audit-logs?page={page}&pageSize={pageSize}", ct).ConfigureAwait(false);
        }
        catch
        {
            return new PagedResult<AuditLogEntryViewModel>
            {
                Items = [],
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
