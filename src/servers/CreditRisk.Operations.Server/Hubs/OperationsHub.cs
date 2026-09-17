// File: src/servers/CreditRisk.Operations.Server/Hubs/OperationsHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CreditRisk.Operations.Server.Hubs;

/// <summary>
/// SignalR hub for real-time operations dashboard updates.
/// Clients join role-based groups on connection.
/// Requires authentication — JWT validated by Keycloak.
/// </summary>
[Authorize]
public sealed class OperationsHub : Hub
{
    private readonly ILogger<OperationsHub> _logger;

    public OperationsHub(ILogger<OperationsHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("OperationsHub client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Adds the connected client to the role-based group.
    /// Called by the Blazor client on connection.
    /// </summary>
    public async Task JoinRoleGroup(string role)
    {
        string allowedRole = role switch
        {
            "desk-operator" => "role:desk-operator",
            "compliance-analyst" => "role:compliance-analyst",
            "administrator" => "role:administrator",
            _ => throw new HubException($"Unknown role: {role}")
        };

        await Groups.AddToGroupAsync(Context.ConnectionId, allowedRole).ConfigureAwait(false);
        _logger.LogInformation("Connection {ConnectionId} joined group {Role}", Context.ConnectionId, allowedRole);
    }

    /// <summary>Removes the client from all role groups on disconnect.</summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "role:desk-operator").ConfigureAwait(false);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "role:compliance-analyst").ConfigureAwait(false);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "role:administrator").ConfigureAwait(false);
        _logger.LogInformation("OperationsHub client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }
}

/// <summary>
/// Notification payload for AML alerts sent via SignalR.
/// </summary>
public sealed record AmlAlertNotification(
    Guid AlertId,
    Guid TransactionId,
    Guid CustomerId,
    string AlertType,
    string Severity,
    decimal TransactionAmount,
    DateTimeOffset CreatedAt,
    string Description);

/// <summary>
/// Notification payload for flagged transactions.
/// </summary>
public sealed record TransactionFlaggedNotification(
    Guid TransactionId,
    Guid CustomerId,
    string Reason,
    List<string> AlertTypes,
    DateTimeOffset FlaggedAt);

/// <summary>
/// Notification payload for dashboard updates.
/// </summary>
public sealed record DashboardUpdateNotification(
    int NewAlertsCount,
    int PendingAlertsCount,
    int CriticalAlertsCount,
    DateTimeOffset UpdatedAt);
