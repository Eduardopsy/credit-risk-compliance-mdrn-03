namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// DTO for reviewing an AML alert.
/// </summary>
public sealed class ReviewAlertRequest
{
    public string Decision { get; set; } = "Dismissed"; // Dismissed, Escalated, ConfirmedFraud
    public string Notes { get; set; } = string.Empty;
    public string ReviewedBy { get; set; } = string.Empty;
}
