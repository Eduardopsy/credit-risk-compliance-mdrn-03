namespace CreditRisk.Operations.Client.Models;

/// <summary>
/// Aggregated metrics for Risk Dashboard.
/// </summary>
public sealed class DashboardMetricsViewModel
{
    public int ActiveProposals { get; set; }
    public int PendingReview { get; set; }
    public int ApprovedToday { get; set; }
    public int RejectedToday { get; set; }
    public int RatingACount { get; set; }
    public int RatingBCount { get; set; }
    public int RatingCCount { get; set; }
    public int RatingDCount { get; set; }
    public int RatingECount { get; set; }
}
