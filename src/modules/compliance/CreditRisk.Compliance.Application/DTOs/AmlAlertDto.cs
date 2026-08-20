// File: src/modules/compliance/CreditRisk.Compliance.Application/DTOs/AmlAlertDto.cs
namespace CreditRisk.Compliance.Application.DTOs;

public sealed record AmlAlertDto
{
    public required Guid Id { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string AlertType { get; init; }
    public required string Severity { get; init; }
    public required decimal TransactionAmount { get; init; }
    public required string Status { get; init; }
    public required string? ReviewedBy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
