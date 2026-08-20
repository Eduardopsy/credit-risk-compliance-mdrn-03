// File: src/modules/iam/CreditRisk.IAM.Application/Commands/Logout/LogoutCommand.cs
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Commands.Logout;

public sealed record LogoutCommand(string Jti, TimeSpan ExpiresIn);
