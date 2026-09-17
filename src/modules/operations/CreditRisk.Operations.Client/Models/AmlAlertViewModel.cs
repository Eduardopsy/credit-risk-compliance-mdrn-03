namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// ViewModel representing an AML/CFT Alert in the UI.
/// </summary>
public sealed class AmlAlertViewModel
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerDocument { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public decimal TransactionAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsRead { get; set; }
    public string? ReviewerComments { get; set; }

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
