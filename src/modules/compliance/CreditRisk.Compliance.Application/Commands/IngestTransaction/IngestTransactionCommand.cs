// File: src/modules/compliance/CreditRisk.Compliance.Application/Commands/IngestTransaction/IngestTransactionCommand.cs
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.Compliance.Application.Commands.IngestTransaction;

public sealed record IngestTransactionCommand(
    Guid TransactionId,
    string CustomerDocument,
    string CustomerDocumentType,
    decimal Amount,
    string TransactionType,
    string Channel,
    DateTimeOffset TransactionDate,
    string OriginAccountId,
    string DestinationAccountId,
    Guid CorrelationId);
