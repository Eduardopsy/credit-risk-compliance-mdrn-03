// File: src/servers/CreditRisk.Operations.Server/Hubs/OperationsHub.cs
namespace CreditRisk.Operations.Server.Hubs;

using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR hub for real-time operations updates.
/// Broadcasts compliance alerts, transaction flags, and operational events to connected clients.
/// </summary>
public sealed class OperationsHub : Hub
{
    private readonly ILogger<OperationsHub> _logger;

    public OperationsHub(ILogger<OperationsHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcasts an AML alert to all connected clients.
    /// Called by AlertNotificationService when AmlAlertCreatedEvent is received.
    /// </summary>
    public async Task BroadcastAmlAlert(AmlAlertNotification alert)
    {
        _logger.LogInformation(
            "Broadcasting AML alert AlertId={AlertId} Severity={Severity}",
            alert.AlertId, alert.Severity);

        await Clients.All.SendAsync("ReceiveAmlAlert", alert);
    }

    /// <summary>
    /// Broadcasts an urgent alert (Critical severity) to all connected clients.
    /// Used for immediate escalation notifications.
    /// </summary>
    public async Task BroadcastUrgentAlert(AmlAlertNotification alert)
    {
        _logger.LogWarning(
            "Broadcasting URGENT alert AlertId={AlertId} Severity={Severity}",
            alert.AlertId, alert.Severity);

        await Clients.All.SendAsync("ReceiveUrgentAlert", alert);
    }

    /// <summary>
    /// Notifies compliance officers of a flagged transaction.
    /// </summary>
    public async Task NotifyTransactionFlagged(TransactionFlaggedNotification notification)
    {
        _logger.LogWarning(
            "Notifying transaction flagged TransactionId={TransactionId} Reason={Reason}",
            notification.TransactionId, notification.Reason);

        await Clients.All.SendAsync("ReceiveTransactionFlagged", notification);
    }

    /// <summary>
    /// Sends a dashboard update with alert statistics.
    /// </summary>
    public async Task UpdateDashboard(DashboardUpdateNotification update)
    {
        _logger.LogDebug("Updating dashboard with {AlertCount} alerts", update.NewAlertsCount);
        await Clients.All.SendAsync("ReceiveDashboardUpdate", update);
    }
}

/// <summary>
/// Notification payload for AML alerts sent via SignalR.
/// </summary>
public sealed record AmlAlertNotification(
    /// <summary>Unique alert identifier.</summary>
    Guid AlertId,
    /// <summary>Transaction that triggered the alert.</summary>
    Guid TransactionId,
    /// <summary>Customer involved in the flagged transaction.</summary>
    Guid CustomerId,
    /// <summary>Type of AML rule that triggered (PepMatch, Structuring, etc.).</summary>
    string AlertType,
    /// <summary>Severity level (Low, Medium, High, Critical).</summary>
    string Severity,
    /// <summary>Amount of the flagged transaction.</summary>
    decimal TransactionAmount,
    /// <summary>When the alert was created.</summary>
    DateTimeOffset CreatedAt,
    /// <summary>Description of the alert reason.</summary>
    string Description);

/// <summary>
/// Notification payload for flagged transactions.
/// </summary>
public sealed record TransactionFlaggedNotification(
    /// <summary>Transaction identifier.</summary>
    Guid TransactionId,
    /// <summary>Customer involved.</summary>
    Guid CustomerId,
    /// <summary>Reason for flagging.</summary>
    string Reason,
    /// <summary>Alert types that triggered the flag.</summary>
    List<string> AlertTypes,
    /// <summary>When the flag was created.</summary>
    DateTimeOffset FlaggedAt);

/// <summary>
/// Notification payload for dashboard updates.
/// </summary>
public sealed record DashboardUpdateNotification(
    /// <summary>Number of new alerts since last update.</summary>
    int NewAlertsCount,
    /// <summary>Number of alerts pending review.</summary>
    int PendingAlertsCount,
    /// <summary>Number of critical alerts.</summary>
    int CriticalAlertsCount,
    /// <summary>When the update was created.</summary>
    DateTimeOffset UpdatedAt);
