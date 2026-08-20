// File: src/shared/CreditRisk.Shared.Kernel/Result/PagedResult.cs
namespace CreditRisk.Shared.Kernel.Result;

/// <summary>
/// Represents a paginated result set.
/// This is the CANONICAL definition — do NOT redefine PagedResult&lt;T&gt; in any
/// Application query file. Always import from CreditRisk.Shared.Kernel.Result.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
