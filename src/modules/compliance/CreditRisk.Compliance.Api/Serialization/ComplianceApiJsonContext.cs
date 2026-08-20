// File: src/modules/compliance/CreditRisk.Compliance.Api/Serialization/ComplianceApiJsonContext.cs
using System.Text.Json.Serialization;
using CreditRisk.Compliance.Application.DTOs;

namespace CreditRisk.Compliance.Api.Serialization;

[JsonSerializable(typeof(IngestTransactionRequest))]
[JsonSerializable(typeof(TransactionDto))]
[JsonSerializable(typeof(AmlAlertDto))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
internal sealed partial class ComplianceApiJsonContext : JsonSerializerContext
{
}
