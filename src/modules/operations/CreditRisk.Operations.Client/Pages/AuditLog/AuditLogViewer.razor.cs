// File: src/modules/operations/CreditRisk.Operations.Client/Pages/AuditLog/AuditLogViewer.razor.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.AuditLog;

public sealed partial class AuditLogViewer
{
    [Inject] private ApiClient Api { get; set; } = null!;

    private MudDataGrid<AuditLogEntryViewModel> _dataGrid = null!;
    private string _searchString = string.Empty;

    private async Task<GridData<AuditLogEntryViewModel>> ServerReloadAsync(GridState<AuditLogEntryViewModel> state)
    {
        int pageNumber = state.Page + 1;
        int pageSize = state.PageSize > 0 ? state.PageSize : 20;

        var result = await Api.GetAuditLogsAsync(pageNumber, pageSize);
        var items = result?.Items ?? [];

        if (!string.IsNullOrWhiteSpace(_searchString))
        {
            items = items.Where(l =>
                l.Action.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                l.EntityType.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                l.PerformedBy.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                l.Details.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        return new GridData<AuditLogEntryViewModel>
        {
            TotalItems = result?.TotalCount ?? items.Count,
            Items = items
        };
    }

    private Task OnSearchChanged(string text)
    {
        _searchString = text;
        return _dataGrid.ReloadServerData();
    }

    private Task OnSearchClear()
    {
        _searchString = string.Empty;
        return _dataGrid.ReloadServerData();
    }
}
