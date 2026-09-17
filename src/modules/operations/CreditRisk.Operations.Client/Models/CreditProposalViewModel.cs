namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// ViewModel representing a Credit Proposal in the UI.
/// </summary>
public sealed class CreditProposalViewModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal RequestedLimit { get; set; }
    public decimal? ApprovedLimit { get; set; }
    public string? RiskRating { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EvaluatedAt { get; set; }

    /// <summary>
    /// Returns masked document for LGPD/compliance (e.g. 529.***.***-25).
    /// </summary>
    public string MaskedDocument
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CustomerDocument))
                return string.Empty;

            var cleaned = new string(CustomerDocument.Where(char.IsDigit).ToArray());
            if (cleaned.Length == 11) // CPF
            {
                return $"{cleaned[..3]}.***.***-{cleaned[^2..]}";
            }
            if (cleaned.Length == 14) // CNPJ
            {
                return $"{cleaned[..2]}.***.***/****-{cleaned[^2..]}";
            }
            return CustomerDocument;
        }
    }
}
