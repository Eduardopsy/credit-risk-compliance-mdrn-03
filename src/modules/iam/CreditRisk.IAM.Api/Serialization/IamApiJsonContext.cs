// File: src/modules/iam/CreditRisk.IAM.Api/Serialization/IamApiJsonContext.cs
using System.Text.Json.Serialization;
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.Shared.Kernel.Common;

namespace CreditRisk.IAM.Api.Serialization;

[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(CreateUserRequest))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
internal sealed partial class IamApiJsonContext : JsonSerializerContext
{
}
