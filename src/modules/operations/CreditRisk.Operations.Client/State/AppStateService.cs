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
