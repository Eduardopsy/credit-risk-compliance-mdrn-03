// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Dashboard/OperationsDashboard.razor.cs
using System.Timers;
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.Dashboard;

public sealed partial class OperationsDashboard : IDisposable
{
    [Inject] private AppStateService AppState { get; set; } = null!;
    [Inject] private ApiClient Api { get; set; } = null!;

    private OperationsMetricsViewModel? _metrics;
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
            Console.WriteLine($"[OperationsDashboard] Error loading metrics: {ex.Message}");
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
        _metrics = await Api.GetOperationsMetricsAsync();
        StateHasChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private Color GetHealthColor(string? health) => health?.ToLowerInvariant() switch
    {
        "healthy" => Color.Success,
        "degraded" => Color.Warning,
        _ => Color.Error
    };

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
