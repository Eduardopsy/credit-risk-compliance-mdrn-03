// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Proposals/ProposalList.razor.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.Proposals;

public sealed partial class ProposalList
{
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private MudDataGrid<CreditProposalViewModel> _dataGrid = null!;
    private string _searchString = string.Empty;

    private async Task<GridData<CreditProposalViewModel>> ServerReloadAsync(GridState<CreditProposalViewModel> state)
    {
        int pageNumber = state.Page + 1;
        int pageSize = state.PageSize > 0 ? state.PageSize : 20;

        var result = await Api.GetProposalsAsync(pageNumber, pageSize);
        var items = result?.Items ?? [];

        if (!string.IsNullOrWhiteSpace(_searchString))
        {
            items = items.Where(p =>
                p.CustomerName.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                p.Status.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                p.CustomerDocument.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        return new GridData<CreditProposalViewModel>
        {
            TotalItems = result?.TotalCount ?? items.Count,
            Items = items
        };
    }

    private void OnRowClick(DataGridRowClickEventArgs<CreditProposalViewModel> args)
    {
        NavigateToDetail(args.Item.Id);
    }

    private void NavigateToDetail(Guid id)
    {
        Navigation.NavigateTo($"/proposals/{id}");
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
