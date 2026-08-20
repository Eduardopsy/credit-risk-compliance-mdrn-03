// File: src/shared/CreditRisk.Shared.Kernel/CQRS/IQueryHandler.cs
namespace CreditRisk.Shared.Kernel.CQRS;

/// <summary>
/// Marker interface for query handlers that return a typed result.
/// Register with: services.AddScoped&lt;IQueryHandler&lt;TQuery, TResult&gt;, THandler&gt;()
/// </summary>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : notnull
{
    Task<Result.Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
