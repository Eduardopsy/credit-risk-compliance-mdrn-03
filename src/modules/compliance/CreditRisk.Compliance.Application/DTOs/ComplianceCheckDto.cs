// File: src/modules/compliance/CreditRisk.Compliance.Application/DTOs/ComplianceCheckDto.cs
namespace CreditRisk.Compliance.Application.DTOs;

public sealed record CreateComplianceCheckRequest
{
    public required Guid ProposalId { get; init; }
    public required string ApplicantDocument { get; init; }
    public required string ApplicantName { get; init; }
}

public sealed record ComplianceCheckDetails
{
    public required string PepScreening { get; init; }
    public required string SanctionList { get; init; }
    public required string AmlCheck { get; init; }
}

public sealed record ComplianceCheckResponse
{
    public required Guid Id { get; init; }
    public required Guid ProposalId { get; init; }
    public required string Status { get; init; }
    public required ComplianceCheckDetails Checks { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
