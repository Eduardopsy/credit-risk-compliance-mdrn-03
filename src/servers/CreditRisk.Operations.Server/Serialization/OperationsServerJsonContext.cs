// File: src/servers/CreditRisk.Operations.Server/Serialization/OperationsServerJsonContext.cs
using System.Text.Json.Serialization;
using CreditRisk.Operations.Server.Hubs;
using CreditRisk.Shared.Contracts.Compliance.Events;
using CreditRisk.Shared.Contracts.CreditAnalysis.Events;

namespace CreditRisk.Operations.Server.Serialization;

/// <summary>
/// Source-generated JsonSerializerContext for Operations Server and SignalR payloads.
/// </summary>
[JsonSerializable(typeof(AmlAlertCreatedEvent))]
[JsonSerializable(typeof(CreditLimitApprovedEvent))]
[JsonSerializable(typeof(TransactionFlaggedEvent))]
[JsonSerializable(typeof(AmlAlertNotification))]
[JsonSerializable(typeof(TransactionFlaggedNotification))]
[JsonSerializable(typeof(DashboardUpdateNotification))]
public sealed partial class OperationsServerJsonContext : JsonSerializerContext
{
}
