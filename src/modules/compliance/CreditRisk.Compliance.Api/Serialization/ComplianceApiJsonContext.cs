// File: src/modules/compliance/CreditRisk.Compliance.Api/Serialization/ComplianceApiJsonContext.cs
using System.Text.Json.Serialization;
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Compliance.Api.Endpoints;
using CreditRisk.Shared.Kernel.Common;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.Compliance.Api.Serialization;

[JsonSerializable(typeof(IngestTransactionRequest))]
[JsonSerializable(typeof(ReviewAlertRequest))]
[JsonSerializable(typeof(TransactionDto))]
[JsonSerializable(typeof(AmlAlertDto))]
[JsonSerializable(typeof(PagedResult<AmlAlertDto>))]
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
internal sealed partial class ComplianceApiJsonContext : JsonSerializerContext
{
}
