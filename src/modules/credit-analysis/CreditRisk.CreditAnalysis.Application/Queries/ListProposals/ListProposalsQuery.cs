// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Queries/ListProposals/ListProposalsQuery.cs
namespace CreditRisk.CreditAnalysis.Application.Queries.ListProposals;

public sealed record ListProposalsQuery(int Page = 1, int PageSize = 20);
