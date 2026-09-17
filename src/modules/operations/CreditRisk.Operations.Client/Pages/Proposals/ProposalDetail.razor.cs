// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Proposals/ProposalDetail.razor.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components;

namespace CreditRisk.Operations.Client.Pages.Proposals;

public sealed partial class ProposalDetail : IDisposable
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private AppStateService AppState { get; set; } = null!;

    private CreditProposalViewModel? _proposal;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += OnStateChanged;
        await LoadProposalAsync();
    }

    private async Task LoadProposalAsync()
    {
        _isLoading = true;
        try
        {
            // Check in recent proposals first
            _proposal = AppState.RecentProposals.FirstOrDefault(p => p.Id == Id)
                ?? await Api.GetProposalAsync(Id);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void OnStateChanged()
    {
        var updated = AppState.RecentProposals.FirstOrDefault(p => p.Id == Id);
        if (updated is not null)
        {
            _proposal = updated;
            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        AppState.StateChanged -= OnStateChanged;
    }
}
