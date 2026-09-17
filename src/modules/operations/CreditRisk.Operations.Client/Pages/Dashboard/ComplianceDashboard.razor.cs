// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Dashboard/ComplianceDashboard.razor.cs
using System.Timers;
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components;

namespace CreditRisk.Operations.Client.Pages.Dashboard;

public sealed partial class ComplianceDashboard : IDisposable
{
    [Inject] private AppStateService AppState { get; set; } = null!;
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private System.Timers.Timer? _refreshTimer;
    private bool _isDisconnected => AppState.ConnectionStatus == "Disconnected";

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += OnStateChanged;

        try
        {
            await LoadAlertsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ComplianceDashboard] Error loading alerts: {ex.Message}");
        }

        // Auto-refresh every 60 seconds (Rule 5.4.3)
        _refreshTimer = new System.Timers.Timer(60000);
        _refreshTimer.Elapsed += async (sender, e) =>
        {
            try
            {
                await InvokeAsync(LoadAlertsAsync);
            }
            catch
            {
                // Suppress timer exceptions
            }
        };
        _refreshTimer.AutoReset = true;
        _refreshTimer.Enabled = true;
    }

    private async Task LoadAlertsAsync()
    {
        var result = await Api.GetAlertsAsync(1, 20);
        if (result?.Items is not null)
        {
            foreach (var alert in result.Items)
            {
                if (!AppState.PendingAlerts.Any(a => a.Id == alert.Id))
                {
                    AppState.AddAlert(alert);
                }
            }
        }
        StateHasChanged();
    }

    private void OnStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OpenReviewDialog(AmlAlertViewModel alert)
    {
        AppState.MarkAlertRead(alert.Id);
        Navigation.NavigateTo($"/alerts/{alert.Id}");
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
