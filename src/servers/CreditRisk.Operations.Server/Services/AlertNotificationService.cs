// File: src/servers/CreditRisk.Operations.Server/Services/AlertNotificationService.cs
using CreditRisk.Operations.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CreditRisk.Operations.Server.Services;

/// <summary>
/// Service for broadcasting compliance alerts to connected clients via SignalR.
/// Acts as a bridge between message consumers and the OperationsHub.
/// </summary>
public sealed class AlertNotificationService
{
    private readonly IHubContext<OperationsHub> _hubContext;
    private readonly ILogger<AlertNotificationService> _logger;

    public AlertNotificationService(
        IHubContext<OperationsHub> hubContext,
        ILogger<AlertNotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Broadcasts an AML alert to all connected clients.
    /// </summary>
    public async Task BroadcastAmlAlertAsync(
        Guid alertId,
        Guid transactionId,
        Guid customerId,
        string alertType,
        string severity,
        decimal transactionAmount,
        DateTimeOffset createdAt)
    {
        try
        {
            var notification = new AmlAlertNotification(
                AlertId: alertId,
                TransactionId: transactionId,
                CustomerId: customerId,
                AlertType: alertType,
                Severity: severity,
                TransactionAmount: transactionAmount,
                CreatedAt: createdAt,
                Description: $"AML Alert: {alertType} detected for transaction {transactionAmount:C}");

            // For critical alerts, use UrgentAlert channel
            if (severity == "Critical")
            {
                _logger.LogWarning(
                    "Broadcasting CRITICAL alert AlertId={AlertId} TransactionId={TransactionId}",
                    alertId, transactionId);

                await _hubContext.Clients.All.SendAsync("ReceiveUrgentAlert", notification);
            }
            else
            {
                _logger.LogInformation(
                    "Broadcasting AML alert AlertId={AlertId} Severity={Severity}",
                    alertId, severity);

                await _hubContext.Clients.All.SendAsync("ReceiveAmlAlert", notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting AML alert AlertId={AlertId}", alertId);
            throw;
        }
    }

    /// <summary>
    /// Broadcasts transaction flagging notification.
    /// </summary>
    public async Task BroadcastTransactionFlaggedAsync(
        Guid transactionId,
        Guid customerId,
        string reason,
        List<string> alertTypes)
    {
        try
        {
            var notification = new TransactionFlaggedNotification(
                TransactionId: transactionId,
                CustomerId: customerId,
                Reason: reason,
                AlertTypes: alertTypes,
                FlaggedAt: DateTimeOffset.UtcNow);

            _logger.LogWarning(
                "Broadcasting transaction flagged TransactionId={TransactionId} Reason={Reason}",
                transactionId, reason);

            await _hubContext.Clients.All.SendAsync("ReceiveTransactionFlagged", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting transaction flagged TransactionId={TransactionId}", transactionId);
            throw;
        }
    }

    /// <summary>
    /// Updates dashboard with current alert statistics.
    /// </summary>
    public async Task UpdateDashboardAsync(
        int newAlertsCount,
        int pendingAlertsCount,
        int criticalAlertsCount)
    {
        try
        {
            var update = new DashboardUpdateNotification(
                NewAlertsCount: newAlertsCount,
                PendingAlertsCount: pendingAlertsCount,
                CriticalAlertsCount: criticalAlertsCount,
                UpdatedAt: DateTimeOffset.UtcNow);

            _logger.LogDebug(
                "Updating dashboard - New: {New} Pending: {Pending} Critical: {Critical}",
                newAlertsCount, pendingAlertsCount, criticalAlertsCount);

            await _hubContext.Clients.All.SendAsync("ReceiveDashboardUpdate", update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dashboard");
            throw;
        }
    }
}
