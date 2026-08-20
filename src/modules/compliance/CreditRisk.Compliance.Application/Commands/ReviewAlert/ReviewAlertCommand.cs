// File: src/modules/compliance/CreditRisk.Compliance.Application/Commands/ReviewAlert/ReviewAlertCommand.cs
using CreditRisk.Compliance.Application.DTOs;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.Compliance.Application.Commands.ReviewAlert;

public sealed record ReviewAlertCommand(Guid AlertId, string NewStatus, string ReviewedBy);
