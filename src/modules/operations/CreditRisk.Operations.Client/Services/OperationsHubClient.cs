// File: src/modules/operations/CreditRisk.Operations.Client/Services/OperationsHubClient.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.State;
using CreditRisk.Shared.Contracts.Compliance.Events;
using CreditRisk.Shared.Contracts.CreditAnalysis.Events;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    private readonly string _hubUrl = configuration["HubUrl"] ?? "http://localhost:5003/hubs/operations";

    /// <summary>Starts the SignalR connection and registers event handlers.</summary>
    public async Task StartAsync(string userRole, CancellationToken cancellationToken = default)
    {
        if (_connection is not null && _connection.State == HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        try
                        {
                            var result = await tokenProvider.RequestAccessToken();
                            return result.TryGetToken(out var token) ? token.Value : null;
                        }
                        catch
                        {
                            return null;
                        }
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

            _connection.On<TransactionFlaggedEvent>("ReceiveTransactionFlagged", flagged =>
            {
                appState.AddAlert(new AmlAlertViewModel
                {
                    Id = Guid.NewGuid(),
                    TransactionId = flagged.TransactionId,
                    CustomerId = flagged.CustomerId,
                    AlertType = string.Join(", ", flagged.TriggeredRules),
                    Severity = "High",
                    Description = flagged.FlagReason,
                    CreatedAt = flagged.FlaggedAt,
                    IsRead = false
                });
            });

            await _connection.StartAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                await _connection.InvokeAsync("JoinRoleGroup", userRole, cancellationToken).ConfigureAwait(false);
            }

            appState.SetConnectionStatus("Connected");
            logger.LogInformation("SignalR connected. Role={Role}", userRole);
        }
        catch (Exception ex)
        {
            appState.SetConnectionStatus("Disconnected");
            logger.LogWarning("Failed to connect to SignalR hub: {Message}", ex.Message);
        }
    }

    /// <summary>Gets the current connection state.</summary>
    public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
    }
}
