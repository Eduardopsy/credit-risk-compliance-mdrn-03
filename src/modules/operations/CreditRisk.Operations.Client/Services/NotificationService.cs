// File: src/modules/operations/CreditRisk.Operations.Client/Services/NotificationService.cs
using MudBlazor;

namespace CreditRisk.Operations.Client.Services;

/// <summary>
/// Service wrapping MudBlazor's ISnackbar for unified application alerts and notifications.
/// </summary>
public sealed class NotificationService(ISnackbar snackbar)
{
    public void NotifySuccess(string message) => snackbar.Add(message, Severity.Success);

    public void NotifyWarning(string message) => snackbar.Add(message, Severity.Warning);

    public void NotifyError(string message) => snackbar.Add(message, Severity.Error);

    public void NotifyInfo(string message) => snackbar.Add(message, Severity.Info);
}
