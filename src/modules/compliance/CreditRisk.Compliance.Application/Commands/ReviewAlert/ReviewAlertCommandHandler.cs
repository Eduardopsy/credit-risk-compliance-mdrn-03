// File: src/modules/compliance/CreditRisk.Compliance.Application/Commands/ReviewAlert/ReviewAlertCommandHandler.cs
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Compliance.Domain.Enums;
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.Compliance.Application.Commands.ReviewAlert;

public sealed class ReviewAlertCommandHandler(IAmlAlertRepository alertRepository) : ICommandHandler<ReviewAlertCommand, AmlAlertDto>
{
    public async Task<Result<AmlAlertDto>> HandleAsync(ReviewAlertCommand command, CancellationToken cancellationToken = default)
    {
        var alert = await alertRepository.GetByIdAsync(command.AlertId, cancellationToken).ConfigureAwait(false);
        if (alert is null)
        {
            return Result<AmlAlertDto>.Failure(Error.NotFound("Alert.NotFound", $"Alert {command.AlertId} not found."));
        }

        if (!Enum.TryParse<AlertStatus>(command.NewStatus, true, out var status))
        {
            return Result<AmlAlertDto>.Failure(Error.Validation("Alert.InvalidStatus", "Invalid alert status."));
        }

        alert.Review(status, command.ReviewedBy);
        await alertRepository.UpdateAsync(alert, cancellationToken).ConfigureAwait(false);

        return Result<AmlAlertDto>.Success(new AmlAlertDto
        {
            Id = alert.Id,
            TransactionId = alert.TransactionId,
            CustomerId = alert.CustomerId,
            AlertType = alert.AlertType,
            Severity = alert.Severity.ToString(),
            TransactionAmount = alert.TransactionAmount,
            Status = alert.Status.ToString(),
            ReviewedBy = alert.ReviewedBy,
            CreatedAt = alert.CreatedAt
        });
    }
}
