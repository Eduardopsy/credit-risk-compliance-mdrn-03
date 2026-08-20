// File: src/modules/iam/CreditRisk.IAM.Application/DTOs/LoginRequest.cs
namespace CreditRisk.IAM.Application.DTOs;

/// <summary>Request body for POST /auth/login.</summary>
public sealed record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string TotpCode { get; init; }   // 6-digit TOTP code
}
