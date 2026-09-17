// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Dashboard/RiskDashboard.razor.cs
using System.Timers;
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.Dashboard;

public sealed partial class RiskDashboard : IDisposable
{
    [Inject] private AppStateService AppState { get; set; } = null!;
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private DashboardMetricsViewModel? _metrics;
    private double[] _ratingChartData = [0, 0, 0, 0, 0];
    private string[] _ratingChartLabels = ["A", "B", "C", "D", "E"];
    private System.Timers.Timer? _refreshTimer;
    private bool _isDisconnected => AppState.ConnectionStatus == "Disconnected";

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += OnStateChanged;

        try
        {
            await LoadMetricsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RiskDashboard] Error loading metrics: {ex.Message}");
        }

        // Auto-refresh every 60 seconds (Rule 5.4.3)
        _refreshTimer = new System.Timers.Timer(60000);
        _refreshTimer.Elapsed += async (sender, e) =>
        {
            try
            {
                await InvokeAsync(LoadMetricsAsync);
            }
            catch
            {
                // Suppress timer exceptions
            }
        };
        _refreshTimer.AutoReset = true;
        _refreshTimer.Enabled = true;
    }

    private async Task LoadMetricsAsync()
    {
        _metrics = await Api.GetDashboardMetricsAsync();
        if (_metrics is not null)
        {
            _ratingChartData = [
                _metrics.RatingACount,
                _metrics.RatingBCount,
                _metrics.RatingCCount,
                _metrics.RatingDCount,
                _metrics.RatingECount
            ];
        }
        StateHasChanged();
    }

    private void OnStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnProposalRowClick(DataGridRowClickEventArgs<CreditProposalViewModel> args)
    {
        Navigation.NavigateTo($"/proposals/{args.Item.Id}");
    }

    public void Dispose()
    {
        AppState.StateChanged -= OnStateChanged;
        if (_refreshTimer is not null)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            _refreshTimer = null;
        }
    }
}
