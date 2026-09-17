namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// Response payload after submitting a proposal (HTTP 202 Accepted).
/// </summary>
public sealed class ProposalAcceptedResponse
{
    public Guid ProposalId { get; set; }
    public string Status { get; set; } = "Submitted";
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
