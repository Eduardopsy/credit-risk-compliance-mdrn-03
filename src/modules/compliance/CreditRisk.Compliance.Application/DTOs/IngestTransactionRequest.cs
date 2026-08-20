// File: src/modules/compliance/CreditRisk.Compliance.Application/DTOs/IngestTransactionRequest.cs
namespace CreditRisk.Compliance.Application.DTOs;

public sealed record IngestTransactionRequest
{
    public required Guid TransactionId { get; init; }
    public required string CustomerDocument { get; init; }
    public required string CustomerDocumentType { get; init; }
    public required decimal Amount { get; init; }
    public required string TransactionType { get; init; }
    public required string Channel { get; init; }
    public required DateTimeOffset TransactionDate { get; init; }
    public required string OriginAccountId { get; init; }
    public required string DestinationAccountId { get; init; }
}
