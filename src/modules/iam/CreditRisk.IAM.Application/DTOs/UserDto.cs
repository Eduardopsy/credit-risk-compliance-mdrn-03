// File: src/modules/iam/CreditRisk.IAM.Application/DTOs/UserDto.cs
namespace CreditRisk.IAM.Application.DTOs;

public sealed record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }
    public required bool IsActive { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
