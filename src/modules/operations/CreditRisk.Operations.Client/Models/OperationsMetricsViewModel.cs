namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// Aggregated metrics for Operations Dashboard.
/// </summary>
public sealed class OperationsMetricsViewModel
{
    public string SystemHealth { get; set; } = "Healthy";
    public int TotalProposalsProcessed { get; set; }
    public int TotalAlertsRaised { get; set; }
    public int ActiveOperators { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public int QueueDepth { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
