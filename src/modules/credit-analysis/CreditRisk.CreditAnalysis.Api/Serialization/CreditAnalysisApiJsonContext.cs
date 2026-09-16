// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Serialization/CreditAnalysisApiJsonContext.cs
using System.Text.Json.Serialization;
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.Shared.Kernel.Common;

namespace CreditRisk.CreditAnalysis.Api.Serialization;

[JsonSerializable(typeof(CreateProposalRequest))]
[JsonSerializable(typeof(CreditProposalDto))]
[JsonSerializable(typeof(ProposalListItemDto))]
[JsonSerializable(typeof(CreditRisk.Shared.Kernel.Result.PagedResult<ProposalListItemDto>))]
[JsonSerializable(typeof(ProposalAcceptedResponse))]
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
internal sealed partial class CreditAnalysisApiJsonContext : JsonSerializerContext
{
}
