// File: src/modules/operations/CreditRisk.Operations.Client/Shared/MainLayout.razor.cs
using System.Security.Claims;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace CreditRisk.Operations.Client.Shared;

public partial class MainLayout : IDisposable
{
    [Inject] private AppStateService AppState { get; set; } = null!;
    [Inject] private OperationsHubClient HubClient { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthStateTask { get; set; }

    private bool _drawerOpen = true;

    private readonly MudTheme _customTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1976D2",
            Secondary = "#424242",
            AppbarBackground = "#1976D2",
            Background = "#F5F5F7"
        }
    };

    protected override async Task OnInitializedAsync()
    {
        AppState.StateChanged += OnStateChanged;

        try
        {
            if (AuthStateTask is not null)
            {
                var authState = await AuthStateTask;
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    var roleClaim = authState.User.FindFirst(ClaimTypes.Role)?.Value 
                        ?? authState.User.FindFirst("roles")?.Value
                        ?? "desk-operator";

                    await HubClient.StartAsync(roleClaim);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainLayout] AuthState check suppressed: {ex.Message}");
        }
    }

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void BeginSignOut()
    {
        Navigation.NavigateToLogout("authentication/logout");
    }

    public void Dispose()
    {
        AppState.StateChanged -= OnStateChanged;
    }
}
