namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// DTO for creating a new Credit Proposal.
/// </summary>
public sealed class CreateProposalRequest
{
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerDocumentType { get; set; } = "CPF";
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public decimal RequestedLimit { get; set; }
    public bool BureauConsentGiven { get; set; }
}
