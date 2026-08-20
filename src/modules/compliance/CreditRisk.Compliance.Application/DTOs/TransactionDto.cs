// File: src/modules/compliance/CreditRisk.Compliance.Application/DTOs/TransactionDto.cs
namespace CreditRisk.Compliance.Application.DTOs;

public sealed record TransactionDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal Amount { get; init; }
    public required string TransactionType { get; init; }
    public required string Channel { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset TransactionDate { get; init; }
}
