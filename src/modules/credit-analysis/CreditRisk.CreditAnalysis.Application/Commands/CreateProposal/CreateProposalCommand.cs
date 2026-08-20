// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Commands/CreateProposal/CreateProposalCommand.cs
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.CreditAnalysis.Application.Commands.CreateProposal;

public sealed record CreateProposalCommand(
    string CustomerDocument,
    string CustomerDocumentType,
    string CustomerName,
    string CustomerEmail,
    decimal MonthlyIncome,
    decimal RequestedLimit,
    string ProposalType,
    bool BureauConsentGiven,
    string BureauConsentIpAddress,
    string CreatedBy,
    Guid CorrelationId);
