// File: src/modules/iam/CreditRisk.IAM.Api/Serialization/IamApiJsonContext.cs
using System.Text.Json.Serialization;
using CreditRisk.IAM.Application.DTOs;

namespace CreditRisk.IAM.Api.Serialization;

[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(CreateUserRequest))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
internal sealed partial class IamApiJsonContext : JsonSerializerContext
{
}
