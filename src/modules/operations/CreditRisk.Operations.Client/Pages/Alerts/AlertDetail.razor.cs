// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Alerts/AlertDetail.razor.cs
using System.Security.Claims;
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.Alerts;

public sealed partial class AlertDetail
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private AppStateService AppState { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    private AmlAlertViewModel? _alert;
    private bool _isLoading = true;
    private bool _isSubmitting;
    private ReviewAlertRequest _reviewRequest = new();

    protected override async Task OnInitializedAsync()
    {
        AppState.MarkAlertRead(Id);
        await LoadAlertAsync();
    }

    private async Task LoadAlertAsync()
    {
        _isLoading = true;
        try
        {
            _alert = AppState.PendingAlerts.FirstOrDefault(a => a.Id == Id)
                ?? await Api.GetAlertAsync(Id);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SubmitReviewAsync()
    {
        if (string.IsNullOrWhiteSpace(_reviewRequest.Notes))
        {
            Snackbar.Add("Please provide justification notes for the review decision.", Severity.Warning);
            return;
        }

        _isSubmitting = true;
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            _reviewRequest.ReviewedBy = authState.User.Identity?.Name ?? "compliance-analyst";

            var updated = await Api.ReviewAlertAsync(Id, _reviewRequest);
            if (updated is not null)
            {
                _alert = updated;
            }
            else if (_alert is not null)
            {
                _alert.Status = _reviewRequest.Decision;
                _alert.ReviewerComments = _reviewRequest.Notes;
            }

            Snackbar.Add("Alert review determination recorded successfully.", Severity.Success);
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to submit review. Please try again.", Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
