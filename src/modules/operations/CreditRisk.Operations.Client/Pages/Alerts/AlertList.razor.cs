// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Alerts/AlertList.razor.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.Alerts;

public sealed partial class AlertList
{
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private AppStateService AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private MudDataGrid<AmlAlertViewModel> _dataGrid = null!;
    private string _searchString = string.Empty;

    private async Task<GridData<AmlAlertViewModel>> ServerReloadAsync(GridState<AmlAlertViewModel> state)
    {
        int pageNumber = state.Page + 1;
        int pageSize = state.PageSize > 0 ? state.PageSize : 20;

        var result = await Api.GetAlertsAsync(pageNumber, pageSize);
        var items = result?.Items ?? [];

        // Prepend any real-time in-memory alerts that are not yet persisted
        var pendingIds = items.Select(x => x.Id).ToHashSet();
        var extraInMemory = AppState.PendingAlerts.Where(a => !pendingIds.Contains(a.Id)).ToList();
        
        var combined = extraInMemory.Concat(items).ToList();

        if (!string.IsNullOrWhiteSpace(_searchString))
        {
            combined = combined.Where(a =>
                a.AlertType.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                a.Severity.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                a.Status.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                a.CustomerDocument.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        return new GridData<AmlAlertViewModel>
        {
            TotalItems = (result?.TotalCount ?? items.Count) + extraInMemory.Count,
            Items = combined
        };
    }

    private void OnRowClick(DataGridRowClickEventArgs<AmlAlertViewModel> args)
    {
        NavigateToDetail(args.Item.Id);
    }

    private void NavigateToDetail(Guid id)
    {
        AppState.MarkAlertRead(id);
        Navigation.NavigateTo($"/alerts/{id}");
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
