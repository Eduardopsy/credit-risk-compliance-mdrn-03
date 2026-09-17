
# SPEC-05 — Front-End
## Credit Risk Compliance Lab — Technical Specification

> **Status:** Authoritative | **Version:** 1.0.0 | **Source:** [`setup.md`](../../setup.md)
> **Depends on:** [`SPEC-01-architecture-core.md`](SPEC-01-architecture-core.md), [`SPEC-02-backend.md`](SPEC-02-backend.md), [`SPEC-04-integration.md`](SPEC-04-integration.md)

---

## Table of Contents

1. [Overview and Responsibilities](#1-overview-and-responsibilities)
2. [Complete Technology Stack](#2-complete-technology-stack)
3. [Directory Structure and Naming Conventions](#3-directory-structure-and-naming-conventions)
4. [Contracts and Interfaces](#4-contracts-and-interfaces)
5. [Business Rules and Invariants](#5-business-rules-and-invariants)
6. [Design Decisions and Architectural Patterns](#6-design-decisions-and-architectural-patterns)
7. [Configuration and Environment Variables](#7-configuration-and-environment-variables)
8. [Detailed Test Scenarios](#8-detailed-test-scenarios)
9. [Acceptance Criteria and Definition of Done](#9-acceptance-criteria-and-definition-of-done)
10. [Local Execution Instructions](#10-local-execution-instructions)

---

## 1. Overview and Responsibilities

### 1.1 Scope

This frente owns the complete Blazor WebAssembly front-end application:

- **`CreditRisk.Operations.Client`**: Blazor WASM SPA with MudBlazor components, OIDC authentication, SignalR client, and all dashboard pages.
- **`CreditRisk.Operations.Server`**: ASP.NET Core host serving the WASM app, SignalR hub, and MassTransit consumers (located in `src/servers/CreditRisk.Operations.Server/`, see SPEC-04).
- **Dashboards**: Risk Dashboard, Compliance Dashboard, Operations Dashboard, Audit Log viewer.
- **Authentication**: OIDC client with Keycloak, role-based UI rendering, JWT bearer token injection.
- **Real-time updates**: SignalR client with automatic reconnection and event-driven state updates.
- **Global state**: `AppStateService` with cascading state pattern.

### 1.2 Boundaries

**Owns:** `src/modules/operations/CreditRisk.Operations.Client/`, `src/servers/CreditRisk.Operations.Server/`.

**Does NOT own:** SignalR hub consumers (SPEC-04), API endpoints (SPEC-02), Docker/CI (SPEC-07), frontend tests (SPEC-06).

**Depends on (from SPEC-01):**
- `CreditRisk.Shared.Contracts`: Event types for SignalR message deserialization

**Depends on (from SPEC-02):**
- REST API endpoints for all data fetching

**Depends on (from SPEC-04):**
- `OperationsHub` SignalR hub for real-time events

---

## 2. Complete Technology Stack

| Component | Package | Version |
|---|---|---|
| Frontend framework | Microsoft.AspNetCore.Components.WebAssembly | 8.0.11 |
| Frontend auth | Microsoft.AspNetCore.Components.WebAssembly.Authentication | 8.0.11 |
| UI components | MudBlazor | 7.15.0 |
| SignalR client | Microsoft.AspNetCore.SignalR.Client | 8.0.11 |
| HTTP client | System.Net.Http.Json | 8.0.1 |
| Server hosting | Microsoft.AspNetCore.Components.WebAssembly.Server | 8.0.11 |
| Server SignalR | Microsoft.AspNetCore.SignalR | 1.1.0 |
| Server MassTransit | MassTransit | 8.3.6 |
| Server MassTransit RabbitMQ | MassTransit.RabbitMQ | 8.3.6 |
| Server Redis backplane | Microsoft.AspNetCore.SignalR.StackExchangeRedis | 8.0.11 |

---

## 3. Directory Structure and Naming Conventions

```
src/modules/operations/
└── CreditRisk.Operations.Client/
    ├── CreditRisk.Operations.Client.csproj
    ├── Program.cs
    ├── App.razor
    ├── _Imports.razor
    ├── wwwroot/
    │   ├── index.html
    │   ├── appsettings.json
    │   └── appsettings.Development.json
    ├── Pages/
    │   ├── Authentication.razor
    │   ├── Dashboard/
    │   │   ├── RiskDashboard.razor
    │   │   ├── RiskDashboard.razor.cs
    │   │   ├── ComplianceDashboard.razor
    │   │   ├── ComplianceDashboard.razor.cs
    │   │   ├── OperationsDashboard.razor
    │   │   └── OperationsDashboard.razor.cs
    │   ├── Proposals/
    │   │   ├── ProposalList.razor
    │   │   ├── ProposalList.razor.cs
    │   │   ├── ProposalDetail.razor
    │   │   ├── ProposalDetail.razor.cs
    │   │   ├── CreateProposal.razor
    │   │   └── CreateProposal.razor.cs
    │   ├── Alerts/
    │   │   ├── AlertList.razor
    │   │   ├── AlertList.razor.cs
    │   │   ├── AlertDetail.razor
    │   │   └── AlertDetail.razor.cs
    │   └── AuditLog/
    │       ├── AuditLogViewer.razor
    │       └── AuditLogViewer.razor.cs
    ├── Shared/
    │   ├── MainLayout.razor
    │   ├── MainLayout.razor.cs
    │   ├── NavMenu.razor
    │   ├── RedirectToLogin.razor
    │   └── NotAuthorized.razor
    ├── Components/
    │   ├── RiskRatingBadge.razor
    │   ├── AlertSeverityChip.razor
    │   ├── ProposalStatusStepper.razor
    │   └── ConnectionStatusIndicator.razor
    ├── Services/
    │   ├── ApiClient.cs
    │   ├── CustomUserFactory.cs
    │   ├── OperationsHubClient.cs
    │   └── NotificationService.cs
    ├── State/
    │   └── AppStateService.cs
    └── Models/
        ├── CreditProposalViewModel.cs
        ├── AmlAlertViewModel.cs
        └── DashboardMetricsViewModel.cs

src/servers/
└── CreditRisk.Operations.Server/
    ├── CreditRisk.Operations.Server.csproj
    ├── Program.cs
    ├── Hubs/
    │   └── OperationsHub.cs
    ├── Consumers/
    │   ├── AmlAlertCreatedEventConsumer.cs
    │   ├── CreditLimitApprovedEventConsumer.cs
    │   └── TransactionFlaggedEventConsumer.cs
    └── Serialization/
        └── OperationsServerJsonContext.cs
```

### 3.1 Naming Conventions

| Construct | Convention | Example |
|---|---|---|
| Page component | `PascalCase.razor` + `PascalCase.razor.cs` | `RiskDashboard.razor` |
| Shared component | `PascalCase.razor` | `RiskRatingBadge.razor` |
| Service | `PascalCase + Service` | `AppStateService` |
| ViewModel | `PascalCase + ViewModel` | `CreditProposalViewModel` |
| Route | `@page "/kebab-case"` | `@page "/risk-dashboard"` |

---

## 4. Contracts and Interfaces

### 4.1 Client `.csproj`

```xml
<!-- File: src/modules/operations/CreditRisk.Operations.Client/CreditRisk.Operations.Client.csproj -->
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <AssemblyName>CreditRisk.Operations.Client</AssemblyName>
    <RootNamespace>CreditRisk.Operations.Client</RootNamespace>
    <PublishAot>false</PublishAot>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Authentication" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" />
    <PackageReference Include="MudBlazor" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../shared/CreditRisk.Shared.Contracts/CreditRisk.Shared.Contracts.csproj" />
  </ItemGroup>
</Project>
```

### 4.2 Client `Program.cs`

```csharp
// File: src/modules/operations/CreditRisk.Operations.Client/Program.cs
using System.Security.Claims;
using CreditRisk.Operations.Client;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// OIDC authentication with Keycloak and CustomUserFactory for role claim mapping
builder.Services.AddOidcAuthentication(options =>
{
    var oidcConfig = builder.Configuration.GetSection("Oidc");
    options.ProviderOptions.Authority = oidcConfig["Authority"] ?? "http://localhost:8080/realms/crcl";
    options.ProviderOptions.MetadataUrl = $"{options.ProviderOptions.Authority}/.well-known/openid-configuration";
    options.ProviderOptions.ClientId = oidcConfig["ClientId"] ?? "crcl-blazor-client";
    options.ProviderOptions.ResponseType = oidcConfig["ResponseType"] ?? "code";

    options.ProviderOptions.DefaultScopes.Clear();
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("roles");

    options.ProviderOptions.RedirectUri = oidcConfig["RedirectUri"] ?? "http://localhost:5003/authentication/login-callback";
    options.ProviderOptions.PostLogoutRedirectUri = oidcConfig["PostLogoutRedirectUri"] ?? "http://localhost:5003/";

    options.UserOptions.RoleClaim = ClaimTypes.Role;
}).AddAccountClaimsPrincipalFactory<CustomUserFactory>();

// HTTP client
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000/api")
});

// MudBlazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// Application services
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<OperationsHubClient>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<NotificationService>();

await builder.Build().RunAsync();
```

### 4.3 `AppStateService.cs`

```csharp
// File: src/modules/operations/CreditRisk.Operations.Client/State/AppStateService.cs
using CreditRisk.Operations.Client.Models;

namespace CreditRisk.Operations.Client.State;

/// <summary>
/// Global application state service. Components subscribe to StateChanged to re-render.
/// Injected as Scoped — one instance per browser tab.
/// </summary>
public sealed class AppStateService
{
    private readonly List<AmlAlertViewModel> _pendingAlerts = [];
    private readonly List<CreditProposalViewModel> _recentProposals = [];

    /// <summary>Raised when any state property changes. Components call StateHasChanged in response.</summary>
    public event Action? StateChanged;

    /// <summary>Gets the current list of pending AML alerts.</summary>
    public IReadOnlyList<AmlAlertViewModel> PendingAlerts => _pendingAlerts.AsReadOnly();

    /// <summary>Gets the recent credit proposals for the current operator.</summary>
    public IReadOnlyList<CreditProposalViewModel> RecentProposals => _recentProposals.AsReadOnly();

    /// <summary>Gets the current SignalR connection status.</summary>
    public string ConnectionStatus { get; private set; } = "Disconnected";

    /// <summary>Gets the count of unread alerts.</summary>
    public int UnreadAlertCount => _pendingAlerts.Count(a => !a.IsRead);

    /// <summary>Adds a new AML alert received from SignalR.</summary>
    public void AddAlert(AmlAlertViewModel alert)
    {
        _pendingAlerts.Insert(0, alert);
        // Keep only last 100 alerts in memory
        if (_pendingAlerts.Count > 100)
            _pendingAlerts.RemoveAt(_pendingAlerts.Count - 1);
        NotifyStateChanged();
    }

    /// <summary>Marks an alert as read.</summary>
    public void MarkAlertRead(Guid alertId)
    {
        var alert = _pendingAlerts.FirstOrDefault(a => a.Id == alertId);
        if (alert is not null)
        {
            alert.IsRead = true;
            NotifyStateChanged();
        }
    }

    /// <summary>Updates the SignalR connection status.</summary>
    public void SetConnectionStatus(string status)
    {
        ConnectionStatus = status;
        NotifyStateChanged();
    }

    /// <summary>Adds or updates a proposal in the recent proposals list.</summary>
    public void UpsertProposal(CreditProposalViewModel proposal)
    {
        int existing = _recentProposals.FindIndex(p => p.Id == proposal.Id);
        if (existing >= 0)
            _recentProposals[existing] = proposal;
        else
            _recentProposals.Insert(0, proposal);

        if (_recentProposals.Count > 50)
            _recentProposals.RemoveAt(_recentProposals.Count - 1);

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
```

### 4.4 `OperationsHubClient.cs`

```csharp
// File: src/modules/operations/CreditRisk.Operations.Client/Services/OperationsHubClient.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.State;
using CreditRisk.Shared.Contracts.Compliance.Events;
using CreditRisk.Shared.Contracts.CreditAnalysis.Events;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;

namespace CreditRisk.Operations.Client.Services;

/// <summary>
/// Manages the SignalR connection to OperationsHub.
/// Handles automatic reconnection with exponential backoff.
/// Subscribes to events and updates AppStateService.
/// </summary>
public sealed class OperationsHubClient(
    IAccessTokenProvider tokenProvider,
    AppStateService appState,
    IConfiguration configuration,
    ILogger<OperationsHubClient> logger) : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly string _hubUrl = configuration["HubUrl"] ?? "/hubs/operations";

    /// <summary>Starts the SignalR connection and registers event handlers.</summary>
    public async Task StartAsync(string userRole, CancellationToken cancellationToken = default)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    var result = await tokenProvider.RequestAccessToken();
                    return result.TryGetToken(out var token) ? token.Value : null;
                };
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        // Connection lifecycle events
        _connection.Reconnecting += error =>
        {
            appState.SetConnectionStatus("Reconnecting...");
            logger.LogWarning("SignalR reconnecting: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            appState.SetConnectionStatus("Connected");
            logger.LogInformation("SignalR reconnected. ConnectionId={ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        _connection.Closed += error =>
        {
            appState.SetConnectionStatus("Disconnected");
            logger.LogWarning("SignalR connection closed: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        // Event handlers
        _connection.On<AmlAlertCreatedEvent>("AmlAlertReceived", alert =>
        {
            appState.AddAlert(new AmlAlertViewModel
            {
                Id = alert.AlertId,
                TransactionId = alert.TransactionId,
                CustomerId = alert.CustomerId,
                AlertType = alert.AlertType,
                Severity = alert.Severity,
                TransactionAmount = alert.TransactionAmount,
                CreatedAt = alert.CreatedAt,
                IsRead = false
            });
        });

        _connection.On<CreditLimitApprovedEvent>("CreditLimitApproved", approval =>
        {
            appState.UpsertProposal(new CreditProposalViewModel
            {
                Id = approval.ProposalId,
                CustomerId = approval.CustomerId,
                ApprovedLimit = approval.ApprovedLimit,
                RiskRating = approval.RiskRating,
                Status = "Approved"
            });
        });

        await _connection.StartAsync(cancellationToken).ConfigureAwait(false);
        await _connection.InvokeAsync("JoinRoleGroup", userRole, cancellationToken).ConfigureAwait(false);

        appState.SetConnectionStatus("Connected");
        logger.LogInformation("SignalR connected. Role={Role}", userRole);
    }

    /// <summary>Gets the current connection state.</summary>
    public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
```

### 4.5 `ApiClient.cs`

```csharp
// File: src/modules/operations/CreditRisk.Operations.Client/Services/ApiClient.cs
using CreditRisk.Operations.Client.Models;
using System.Net.Http.Json;

namespace CreditRisk.Operations.Client.Services;

/// <summary>
/// Typed HTTP client for all back-end API calls.
/// Uses the "crcl-api" named client with JWT bearer token injection.
/// </summary>
public sealed class ApiClient(HttpClient httpClient)
{
    // ── Credit Proposals ────────────────────────────────────────────────────────

    public async Task<CreditProposalViewModel?> GetProposalAsync(
        Guid id, CancellationToken ct = default)
        => await httpClient.GetFromJsonAsync<CreditProposalViewModel>(
            $"api/v1/proposals/{id}", ct).ConfigureAwait(false);

    public async Task<PagedResult<CreditProposalViewModel>?> GetProposalsAsync(
        int page = 1, int pageSize = 20, CancellationToken ct = default)
        => await httpClient.GetFromJsonAsync<PagedResult<CreditProposalViewModel>>(
            $"api/v1/proposals?page={page}&pageSize={pageSize}", ct).ConfigureAwait(false);

    public async Task<ProposalAcceptedResponse?> CreateProposalAsync(
        CreateProposalRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/proposals", request, ct)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProposalAcceptedResponse>(ct)
            .ConfigureAwait(false);
    }

    // ── AML Alerts ──────────────────────────────────────────────────────────────

    public async Task<PagedResult<AmlAlertViewModel>?> GetAlertsAsync(
        int page = 1, int pageSize = 20, CancellationToken ct = default)
        => await httpClient.GetFromJsonAsync<PagedResult<AmlAlertViewModel>>(
            $"api/v1/alerts?page={page}&pageSize={pageSize}", ct).ConfigureAwait(false);

    public async Task<AmlAlertViewModel?> ReviewAlertAsync(
        Guid alertId, ReviewAlertRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/v1/alerts/{alertId}/review", request, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AmlAlertViewModel>(ct).ConfigureAwait(false);
    }
}
```

### 4.5.1 `CustomUserFactory.cs` — OIDC Role Mapping

```csharp
// File: src/modules/operations/CreditRisk.Operations.Client/Services/CustomUserFactory.cs
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace CreditRisk.Operations.Client.Services;

/// <summary>
/// Custom ClaimsPrincipalFactory for Keycloak OIDC authentication.
/// Flattens 'roles' array and 'realm_access.roles' into standard ClaimTypes.Role claims.
/// </summary>
public sealed class CustomUserFactory(IAccessTokenProviderAccessor accessor)
    : AccountClaimsPrincipalFactory<RemoteUserAccount>(accessor)
{
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);

        // Account is null during anonymous initialization in Blazor WASM — must check to avoid NullReferenceException
        if (account is null)
        {
            return user;
        }

        if (user.Identity is ClaimsIdentity claimsIdentity)
        {
            if (account.AdditionalProperties is not null &&
                account.AdditionalProperties.TryGetValue("roles", out var rolesObj))
            {
                AddRolesFromElement(claimsIdentity, rolesObj);
            }

            if (account.AdditionalProperties is not null &&
                account.AdditionalProperties.TryGetValue("realm_access", out var realmAccessObj) &&
                realmAccessObj is JsonElement realmElement &&
                realmElement.ValueKind == JsonValueKind.Object &&
                realmElement.TryGetProperty("roles", out var realmRoles))
            {
                AddRolesFromElement(claimsIdentity, realmRoles);
            }
        }

        return user;
    }

    private static void AddRolesFromElement(ClaimsIdentity identity, object elementObj)
    {
        if (elementObj is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in element.EnumerateArray())
                {
                    var roleStr = role.GetString();
                    if (!string.IsNullOrWhiteSpace(roleStr) && !identity.HasClaim(identity.RoleClaimType, roleStr))
                    {
                        identity.AddClaim(new Claim(identity.RoleClaimType, roleStr));
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                var roleStr = element.GetString();
                if (!string.IsNullOrWhiteSpace(roleStr) && !identity.HasClaim(identity.RoleClaimType, roleStr))
                {
                    identity.AddClaim(new Claim(identity.RoleClaimType, roleStr));
                }
            }
        }
    }
}
```

### 4.6 Dashboard Pages

#### Risk Dashboard

```razor
@* File: src/modules/operations/CreditRisk.Operations.Client/Pages/Dashboard/RiskDashboard.razor *@
@page "/risk-dashboard"
@page "/"
@attribute [Authorize(Roles = "desk-operator,compliance-analyst,administrator")]
@inject AppStateService AppState
@inject ApiClient Api
@inject ISnackbar Snackbar
@implements IDisposable

<PageTitle>Risk Dashboard — Credit Risk Compliance Lab</PageTitle>

<MudText Typo="Typo.h4" Class="mb-4">Risk Dashboard</MudText>

<MudGrid>
    @* KPI Cards *@
    <MudItem xs="12" sm="6" md="3">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.subtitle2" Color="Color.Secondary">Active Proposals</MudText>
            <MudText Typo="Typo.h4">@_metrics?.ActiveProposals</MudText>
        </MudPaper>
    </MudItem>
    <MudItem xs="12" sm="6" md="3">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.subtitle2" Color="Color.Secondary">Pending Review</MudText>
            <MudText Typo="Typo.h4" Color="Color.Warning">@_metrics?.PendingReview</MudText>
        </MudPaper>
    </MudItem>
    <MudItem xs="12" sm="6" md="3">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.subtitle2" Color="Color.Secondary">Approved Today</MudText>
            <MudText Typo="Typo.h4" Color="Color.Success">@_metrics?.ApprovedToday</MudText>
        </MudPaper>
    </MudItem>
    <MudItem xs="12" sm="6" md="3">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.subtitle2" Color="Color.Secondary">Rejected Today</MudText>
            <MudText Typo="Typo.h4" Color="Color.Error">@_metrics?.RejectedToday</MudText>
        </MudPaper>
    </MudItem>

    @* Risk Rating Distribution Chart *@
    <MudItem xs="12" md="6">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.h6" Class="mb-2">Risk Rating Distribution</MudText>
            <MudChart ChartType="ChartType.Donut"
                      InputData="@_ratingChartData"
                      InputLabels="@_ratingChartLabels"
                      Width="300px" Height="300px" />
        </MudPaper>
    </MudItem>

    @* Recent Proposals Table *@
    <MudItem xs="12" md="6">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.h6" Class="mb-2">Recent Proposals</MudText>
            <MudDataGrid T="CreditProposalViewModel"
                         Items="@AppState.RecentProposals"
                         Dense="true"
                         Hover="true"
                         RowClick="@OnProposalRowClick">
                <Columns>
                    <PropertyColumn Property="x => x.Id" Title="ID" />
                    <PropertyColumn Property="x => x.Status" Title="Status" />
                    <TemplateColumn Title="Rating">
                        <CellTemplate>
                            <RiskRatingBadge Rating="@context.Item.RiskRating" />
                        </CellTemplate>
                    </TemplateColumn>
                    <PropertyColumn Property="x => x.RequestedLimit" Title="Limit" Format="C2" />
                </Columns>
            </MudDataGrid>
        </MudPaper>
    </MudItem>
</MudGrid>
```

```csharp
// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Dashboard/RiskDashboard.razor.cs
using CreditRisk.Operations.Client.Models;
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
    private double[] _ratingChartData = [];
    private string[] _ratingChartLabels = ["A", "B", "C", "D", "E"];

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
    }

    private async Task LoadMetricsAsync()
    {
        _metrics = await Api.GetDashboardMetricsAsync();
        _ratingChartData = [
            _metrics?.RatingACount ?? 0,
            _metrics?.RatingBCount ?? 0,
            _metrics?.RatingCCount ?? 0,
            _metrics?.RatingDCount ?? 0,
            _metrics?.RatingECount ?? 0
        ];
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
    }
}
```

#### Compliance Dashboard

```razor
@* File: src/modules/operations/CreditRisk.Operations.Client/Pages/Dashboard/ComplianceDashboard.razor *@
@page "/compliance-dashboard"
@attribute [Authorize(Roles = "compliance-analyst,administrator")]
@inject AppStateService AppState
@inject ApiClient Api
@implements IDisposable

<PageTitle>Compliance Dashboard — Credit Risk Compliance Lab</PageTitle>

<MudText Typo="Typo.h4" Class="mb-4">
    Compliance Dashboard
    @if (AppState.UnreadAlertCount > 0)
    {
        <MudBadge Content="@AppState.UnreadAlertCount" Color="Color.Error" Class="ml-2">
            <MudIcon Icon="@Icons.Material.Filled.NotificationsActive" />
        </MudBadge>
    }
</MudText>

<MudGrid>
    @* Connection Status *@
    <MudItem xs="12">
        <ConnectionStatusIndicator Status="@AppState.ConnectionStatus" />
    </MudItem>

    @* AML Alert Queue *@
    <MudItem xs="12" md="8">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.h6" Class="mb-2">Pending AML Alerts</MudText>
            <MudDataGrid T="AmlAlertViewModel"
                         Items="@AppState.PendingAlerts"
                         Dense="true"
                         Hover="true"
                         SortMode="SortMode.Multiple"
                         Filterable="true">
                <Columns>
                    <PropertyColumn Property="x => x.AlertType" Title="Type" />
                    <TemplateColumn Title="Severity">
                        <CellTemplate>
                            <AlertSeverityChip Severity="@context.Item.Severity" />
                        </CellTemplate>
                    </TemplateColumn>
                    <PropertyColumn Property="x => x.TransactionAmount" Title="Amount" Format="C2" />
                    <PropertyColumn Property="x => x.CreatedAt" Title="Time" Format="HH:mm:ss" />
                    <TemplateColumn Title="Actions">
                        <CellTemplate>
                            <MudButton Variant="Variant.Outlined" Size="Size.Small"
                                       OnClick="@(() => OpenReviewDialog(context.Item))">
                                Review
                            </MudButton>
                        </CellTemplate>
                    </TemplateColumn>
                </Columns>
            </MudDataGrid>
        </MudPaper>
    </MudItem>

    @* Alert Statistics *@
    <MudItem xs="12" md="4">
        <MudPaper Class="pa-4" Elevation="2">
            <MudText Typo="Typo.h6" Class="mb-2">Alert Statistics (24h)</MudText>
            <MudList Dense="true">
                <MudListItem Icon="@Icons.Material.Filled.Warning" IconColor="Color.Error">
                    Critical: @AppState.PendingAlerts.Count(a => a.Severity == "Critical")
                </MudListItem>
                <MudListItem Icon="@Icons.Material.Filled.Warning" IconColor="Color.Warning">
                    High: @AppState.PendingAlerts.Count(a => a.Severity == "High")
                </MudListItem>
                <MudListItem Icon="@Icons.Material.Filled.Info" IconColor="Color.Info">
                    Medium: @AppState.PendingAlerts.Count(a => a.Severity == "Medium")
                </MudListItem>
            </MudList>
        </MudPaper
>
    </MudItem>
</MudGrid>

@code {
    private async Task OpenReviewDialog(AmlAlertViewModel alert)
    {
        AppState.MarkAlertRead(alert.Id);
        // Dialog implementation in AlertDetail page
        Navigation.NavigateTo($"/alerts/{alert.Id}");
    }
}
```

### 4.7 `CreateProposal.razor` — Form with FluentValidation

```razor
@* File: src/modules/operations/CreditRisk.Operations.Client/Pages/Proposals/CreateProposal.razor *@
@page "/proposals/create"
@attribute [Authorize(Roles = "desk-operator,administrator")]
@inject ApiClient Api
@inject NavigationManager Navigation
@inject ISnackbar Snackbar

<PageTitle>New Credit Proposal</PageTitle>

<MudText Typo="Typo.h5" Class="mb-4">New Credit Proposal</MudText>

<MudForm @ref="_form" Model="@_request" Validation="@_validator.ValidateValue">
    <MudGrid>
        <MudItem xs="12" md="6">
            <MudTextField @bind-Value="_request.CustomerDocument"
                          Label="CPF / CNPJ"
                          For="@(() => _request.CustomerDocument)"
                          Immediate="true"
                          Required="true" />
        </MudItem>
        <MudItem xs="12" md="6">
            <MudSelect @bind-Value="_request.CustomerDocumentType"
                       Label="Document Type"
                       For="@(() => _request.CustomerDocumentType)">
                <MudSelectItem Value="@("CPF")">CPF (Individual)</MudSelectItem>
                <MudSelectItem Value="@("CNPJ")">CNPJ (Legal Entity)</MudSelectItem>
            </MudSelect>
        </MudItem>
        <MudItem xs="12" md="6">
            <MudTextField @bind-Value="_request.CustomerName"
                          Label="Full Name"
                          For="@(() => _request.CustomerName)"
                          Required="true" />
        </MudItem>
        <MudItem xs="12" md="6">
            <MudTextField @bind-Value="_request.CustomerEmail"
                          Label="Email"
                          For="@(() => _request.CustomerEmail)"
                          InputType="InputType.Email"
                          Required="true" />
        </MudItem>
        <MudItem xs="12" md="6">
            <MudNumericField @bind-Value="_request.MonthlyIncome"
                             Label="Monthly Income (BRL)"
                             For="@(() => _request.MonthlyIncome)"
                             Format="C2"
                             Min="0"
                             Required="true" />
        </MudItem>
        <MudItem xs="12" md="6">
            <MudNumericField @bind-Value="_request.RequestedLimit"
                             Label="Requested Credit Limit (BRL)"
                             For="@(() => _request.RequestedLimit)"
                             Format="C2"
                             Min="1"
                             Max="500000"
                             Required="true" />
        </MudItem>
        <MudItem xs="12">
            <MudCheckBox @bind-Value="_request.BureauConsentGiven"
                         For="@(() => _request.BureauConsentGiven)"
                         Label="I confirm the customer has given explicit consent for credit bureau queries (LGPD Art. 7)" />
        </MudItem>
        <MudItem xs="12">
            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       OnClick="@SubmitAsync"
                       Disabled="@_isSubmitting">
                @if (_isSubmitting)
                {
                    <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                }
                Submit Proposal
            </MudButton>
            <MudButton Variant="Variant.Text" OnClick="@Cancel" Class="ml-2">Cancel</MudButton>
        </MudItem>
    </MudGrid>
</MudForm>
```

```csharp
// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Proposals/CreateProposal.razor.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.Proposals;

public sealed partial class CreateProposal
{
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private MudForm _form = null!;
    private CreateProposalRequest _request = new();
    private bool _isSubmitting;
    private readonly CreateProposalRequestClientValidator _validator = new();

    private async Task SubmitAsync()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _isSubmitting = true;
        try
        {
            var result = await Api.CreateProposalAsync(_request);
            Snackbar.Add($"Proposal submitted successfully. ID: {result?.ProposalId}", Severity.Success);
            Navigation.NavigateTo($"/proposals/{result?.ProposalId}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
        {
            Snackbar.Add("Validation failed. Please check the form.", Severity.Error);
        }
        catch (Exception)
        {
            Snackbar.Add("An unexpected error occurred. Please try again.", Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel() => Navigation.NavigateTo("/proposals");
}

/// <summary>Client-side FluentValidation validator for CreateProposalRequest.</summary>
internal sealed class CreateProposalRequestClientValidator : AbstractValidator<CreateProposalRequest>
{
    public CreateProposalRequestClientValidator()
    {
        RuleFor(x => x.CustomerDocument).NotEmpty().WithMessage("Document is required.");
        RuleFor(x => x.CustomerDocumentType).Must(t => t is "CPF" or "CNPJ").WithMessage("Select CPF or CNPJ.");
        RuleFor(x => x.CustomerName).NotEmpty().MinimumLength(3).WithMessage("Full name is required.");
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
        RuleFor(x => x.MonthlyIncome).GreaterThan(0).WithMessage("Monthly income must be greater than zero.");
        RuleFor(x => x.RequestedLimit).GreaterThan(0).LessThanOrEqualTo(500_000).WithMessage("Limit must be between R$ 1 and R$ 500,000.");
        RuleFor(x => x.BureauConsentGiven).Equal(true).WithMessage("Bureau consent is required to proceed.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CreateProposalRequest>
            .CreateWithOptions((CreateProposalRequest)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}
```

### 4.8 `ConnectionStatusIndicator.razor`

```razor
@* File: src/modules/operations/CreditRisk.Operations.Client/Components/ConnectionStatusIndicator.razor *@
@inject AppStateService AppState
@implements IDisposable

<MudChip T="string"
         Color="@GetColor()"
         Icon="@GetIcon()"
         Size="Size.Small">
    @AppState.ConnectionStatus
</MudChip>

@code {
    [Parameter] public string Status { get; set; } = "Disconnected";

    protected override void OnInitialized()
    {
        AppState.StateChanged += OnStateChanged;
    }

    private Color GetColor() => AppState.ConnectionStatus switch
    {
        "Connected" => Color.Success,
        "Reconnecting..." => Color.Warning,
        _ => Color.Error
    };

    private string GetIcon() => AppState.ConnectionStatus switch
    {
        "Connected" => Icons.Material.Filled.Wifi,
        "Reconnecting..." => Icons.Material.Filled.WifiFind,
        _ => Icons.Material.Filled.WifiOff
    };

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => AppState.StateChanged -= OnStateChanged;
}
```

### 4.9 `wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "http://localhost:5000/api",
  "HubUrl": "http://localhost:5003/hubs/operations",
  "Oidc": {
    "Authority": "http://localhost:8080/realms/crcl",
    "ClientId": "crcl-blazor-client",
    "ResponseType": "code",
    "DefaultScopes": ["openid", "profile", "email", "roles"],
    "PostLogoutRedirectUri": "http://localhost:5003/",
    "RedirectUri": "http://localhost:5003/authentication/login-callback"
  }
}
```

---

## 5. Business Rules and Invariants

### 5.1 Authentication Rules

1. All pages except `/authentication/*` require authentication — unauthenticated users are redirected to Keycloak login.
2. Role-based page access is enforced via `[Authorize(Roles = "...")]` attribute. Roles from Keycloak are mapped into `ClaimTypes.Role` via `CustomUserFactory`.
3. Desk operators can only see their own proposals — the API enforces this, but the UI must not show other operators' data.
4. Compliance analysts see all alerts and transactions.
5. Administrators see everything.

### 5.2 Form Validation Rules

1. Client-side FluentValidation runs on every field change (`Immediate="true"`).
2. The submit button is disabled while `_isSubmitting = true`.
3. Bureau consent checkbox must be checked before submission — this is a LGPD requirement.
4. CPF/CNPJ format validation runs client-side; check digit validation runs server-side.

### 5.3 Real-Time Update Rules

1. SignalR connection is established on first navigation to a dashboard page.
2. Automatic reconnection uses exponential backoff: 0s, 2s, 5s, 10s, 30s.
3. `ConnectionStatusIndicator` reflects the current connection state.
4. If SignalR is disconnected, dashboards show stale data with a warning banner.
5. AML alerts received via SignalR are prepended to the alert list — newest first.

### 5.4 Dashboard Layout Rules

1. Risk Dashboard is the default landing page for desk operators (mapped to `/` and `/risk-dashboard`).
2. Compliance Dashboard is the default landing page for compliance analysts.
3. All dashboards auto-refresh every 60 seconds via `System.Timers.Timer`.
4. `MudDataGrid` pagination: default 20 rows per page.
5. Sensitive data (CPF, CNPJ) is masked in the UI: `529.***.***.25`.

---

## 6. Design Decisions and Architectural Patterns

### 6.1 Blazor WASM over Blazor Server

**Decision:** Blazor WebAssembly (client-side) is used, not Blazor Server.

**Rationale:** WASM runs entirely in the browser — no persistent server connection required for UI rendering. This reduces server load and enables offline-capable future enhancements. SignalR is used only for real-time push notifications, not for UI rendering.

### 6.2 Cascading State via `AppStateService`

**Decision:** Global state is managed via `AppStateService` injected as Scoped. Components subscribe to `StateChanged` event.

**Rationale:** Avoids complex state management libraries (Fluxor, BlazorState) while providing reactive updates. The `StateChanged` event pattern is idiomatic Blazor and works correctly with `InvokeAsync(StateHasChanged)` for thread safety.

### 6.3 Code-Behind Pattern for Complex Pages

**Decision:** Complex pages use the code-behind pattern (`Page.razor` + `Page.razor.cs`) with `partial class`.

**Rationale:** Keeps Razor markup clean and separates UI logic from component logic. Simple components (< 30 lines of `@code`) may use inline `@code` blocks.

### 6.4 MudBlazor Component Library

**Decision:** MudBlazor 7.15.0 is the sole UI component library.

**Rationale:** MudBlazor provides a complete Material Design component set including `MudDataGrid`, `MudChart`, `MudDialog`, `MudSnackbar`, and `MudForm` with FluentValidation integration. It is actively maintained and compatible with Blazor WASM.

---

## 7. Configuration and Environment Variables

### 7.1 Client Configuration (`wwwroot/appsettings.json`)

| Key | Type | Dev Default | Description |
|---|---|---|---|
| `ApiBaseUrl` | `string` | `http://localhost:5000/api` | Back-end API base URL |
| `HubUrl` | `string` | `http://localhost:5003/hubs/operations` | SignalR hub URL |
| `Oidc:Authority` | `string` | `http://localhost:8080/realms/crcl` | Keycloak realm URL |
| `Oidc:ClientId` | `string` | `crcl-blazor-client` | Keycloak client ID |
| `Oidc:RedirectUri` | `string` | `http://localhost:5003/authentication/login-callback` | OIDC redirect URI |

### 7.2 Server Configuration (Environment Variables)

| Variable | Type | Dev Default | Description |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `string` | `Development` | Controls CORS, HTTPS redirect |
| `RabbitMQ__Host` | `string` | `rabbitmq` | RabbitMQ hostname |
| `Keycloak__Authority` | `string` | `http://keycloak:8080/realms/crcl` | Keycloak realm URL for JWT validation |
| `SignalR__BackplaneRedis` | `string` | (from `.env`) | Redis backplane connection string |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `string` | `http://otel-collector:4317` | OTel collector endpoint |

---

## 8. Detailed Test Scenarios

See [`SPEC-06-frontend-unit-tests.md`](SPEC-06-frontend-unit-tests.md) for the complete bUnit test suite specification.

Key scenarios to verify manually:

1. **Login flow**: Navigate to `/risk-dashboard` → redirected to Keycloak → login with TOTP → redirected back → dashboard loads.
2. **Role-based access**: Login as `desk-operator` → navigate to `/compliance-dashboard` → see "Not Authorized" page.
3. **Real-time alert**: Trigger a transaction that matches AML rules → alert appears in Compliance Dashboard within 2 seconds.
4. **Reconnection**: Disconnect network → `ConnectionStatusIndicator` shows "Reconnecting..." → reconnect → shows "Connected".
5. **Form validation**: Submit proposal with invalid CPF → field shows error message → submit button remains enabled.

---

## 9. Acceptance Criteria and Definition of Done

- [ ] `dotnet build` succeeds for both Client and Server projects with 0 warnings
- [ ] `Program.cs` configures OIDC with Keycloak, MudBlazor, `AppStateService`, `OperationsHubClient`, `ApiClient`, and `CustomUserFactory`
- [ ] Keycloak realm roles (`roles` and `realm_access.roles`) are mapped into `ClaimTypes.Role`
- [ ] All 4 dashboard pages exist with correct `@page` routes and `[Authorize]` attributes
- [ ] `CreateProposal` form validates CPF/CNPJ format, required fields, bureau consent client-side
- [ ] `AppStateService.StateChanged` triggers re-render in all subscribed components
- [ ] `OperationsHubClient` connects to SignalR hub, joins role group, handles `AmlAlertReceived` and `CreditLimitApproved` events
- [ ] Automatic reconnection with exponential backoff (0s, 2s, 5s, 10s, 30s) is configured
- [ ] `ConnectionStatusIndicator` shows correct color and icon for Connected/Reconnecting/Disconnected
- [ ] `MudDataGrid` pagination defaults to 20 rows per page
- [ ] Sensitive data (CPF/CNPJ) is masked in all UI displays
- [ ] All pages redirect to Keycloak login when unauthenticated
- [ ] `desk-operator` role cannot access `/compliance-dashboard` — shows `NotAuthorized` component
- [ ] `wwwroot/appsettings.json` contains all required OIDC and API configuration keys

---

## 10. Local Execution Instructions

### Step 1: Start Infrastructure and Back-End

```bash
cd credit-risk-compliance-lab
docker compose up -d postgres redis rabbitmq keycloak
# Wait for Keycloak to be healthy (up to 90 seconds)
# Ensure realm is imported:
# docker exec crcl-keycloak /opt/keycloak/bin/kc.sh import --file /opt/keycloak/data/import/realm-export.json

# Ensure database migrations are applied across modules before running services:
# dotnet ef database update --project src/modules/iam/CreditRisk.IAM.Infrastructure --startup-project src/modules/iam/CreditRisk.IAM.Api
# dotnet ef database update --project src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure --startup-project src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api
# dotnet ef database update --project src/modules/compliance/CreditRisk.Compliance.Infrastructure --startup-project src/modules/compliance/CreditRisk.Compliance.Api

# Start Operations Server (SignalR Hub & WASM Host)
dotnet run --project src/servers/CreditRisk.Operations.Server/ \
  --launch-profile Development
```

### Step 2: Run Blazor WASM Client

```bash
# The Client is served directly by the Server project in development at http://localhost:5003
dotnet run --project src/servers/CreditRisk.Operations.Server/ \
  --launch-profile Development
```

### Step 3: Open Browser

```bash
open http://localhost:5003
# You will be redirected to Keycloak login
# Use credentials from seed-data.sql:
#   desk-operator@example.com / TestPassword123!
#   compliance-analyst@example.com / TestPassword123!
```

### Step 4: Verify SignalR Connection

```bash
# In browser DevTools → Network → WS
# Verify WebSocket connection to /hubs/operations is established
# Status should show "Connected" in the UI
```

### Step 5: Publish for Production

```bash
dotnet publish src/modules/operations/CreditRisk.Operations.Client/ \
  -c Release \
  -o /tmp/frontend-publish

# Verify wwwroot output
ls /tmp/frontend-publish/wwwroot/
# Should contain: index.html, _framework/, css/, etc.
```

### Expected Final State

- Browser opens at `http://localhost:5003`
- Redirected to Keycloak login page
- After login, Risk Dashboard loads with MudBlazor components
- `ConnectionStatusIndicator` shows "Connected" (green)
- Creating a proposal navigates to the proposal detail page after 202 Accepted

---

*Cross-references: Connects to APIs defined in [`SPEC-02-backend.md`](SPEC-02-backend.md). Receives SignalR events from hub defined in [`SPEC-04-integration.md`](SPEC-04-integration.md). Test suite specified in [`SPEC-06-frontend-unit-tests.md`](SPEC-06-frontend-unit-tests.md).*