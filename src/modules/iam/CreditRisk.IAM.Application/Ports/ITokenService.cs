// File: src/modules/iam/CreditRisk.IAM.Application/Ports/ITokenService.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Ports;

public interface ITokenService
{
    Task<Result<LoginResponse>> GenerateTokensAsync(User user, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);
}
