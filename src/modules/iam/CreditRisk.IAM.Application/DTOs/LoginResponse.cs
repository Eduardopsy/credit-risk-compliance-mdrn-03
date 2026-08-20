// File: src/modules/iam/CreditRisk.IAM.Application/DTOs/LoginResponse.cs
namespace CreditRisk.IAM.Application.DTOs;

/// <summary>Response body for successful authentication.</summary>
public sealed record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }     // seconds (900 = 15 min)
    public required string TokenType { get; init; }  // "Bearer"
    public required string[] Roles { get; init; }
}
