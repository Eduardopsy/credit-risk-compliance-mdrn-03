// File: src/shared/CreditRisk.Shared.Kernel/Outbox/IOutboxProcessor.cs
namespace CreditRisk.Shared.Kernel.Outbox;

/// <summary>
/// Marker interface for components that process outbox messages.
/// Implementations are typically BackgroundServices registered in DI.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>Start processing outbox messages.</summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Stop processing outbox messages.</summary>
    Task StopAsync(CancellationToken cancellationToken);
}
