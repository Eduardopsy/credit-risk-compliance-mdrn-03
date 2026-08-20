// File: src/modules/iam/CreditRisk.IAM.Application/Commands/Login/LoginCommand.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Commands.Login;

public sealed record LoginCommand(string Email, string Password, string TotpCode);
