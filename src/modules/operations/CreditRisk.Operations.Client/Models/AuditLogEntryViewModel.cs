namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// ViewModel representing an audit log entry.
/// </summary>
public sealed class AuditLogEntryViewModel
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Details { get; set; } = string.Empty;
}
