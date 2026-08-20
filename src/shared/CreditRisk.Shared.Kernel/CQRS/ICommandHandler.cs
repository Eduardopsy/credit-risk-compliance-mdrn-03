// File: src/shared/CreditRisk.Shared.Kernel/CQRS/ICommandHandler.cs
namespace CreditRisk.Shared.Kernel.CQRS;

/// <summary>
/// Marker interface for command handlers that return a typed result.
/// Register with: services.AddScoped&lt;ICommandHandler&lt;TCommand, TResult&gt;, THandler&gt;()
/// </summary>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : notnull
{
    Task<Result.Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
