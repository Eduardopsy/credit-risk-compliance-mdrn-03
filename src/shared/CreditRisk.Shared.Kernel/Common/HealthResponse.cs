// File: src/shared/CreditRisk.Shared.Kernel/Common/HealthResponse.cs
namespace CreditRisk.Shared.Kernel.Common;

/// <summary>
/// Strongly-typed health response record.
/// Used for Native AOT-compliant JSON serialization across all Minimal APIs.
/// Anonymous types (e.g. Results.Ok(new { status = "healthy" })) throw runtime
/// serialization exceptions in Native AOT / CreateSlimBuilder.
/// </summary>
public sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset? Timestamp = null);
