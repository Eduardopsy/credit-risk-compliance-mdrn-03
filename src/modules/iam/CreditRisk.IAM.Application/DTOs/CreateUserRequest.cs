// File: src/modules/iam/CreditRisk.IAM.Application/DTOs/CreateUserRequest.cs
namespace CreditRisk.IAM.Application.DTOs;

/// <summary>Request body for POST /users.</summary>
public sealed record CreateUserRequest
{
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }       // "desk-operator" | "compliance-analyst" | "administrator"
    public required string TemporaryPassword { get; init; }
}
