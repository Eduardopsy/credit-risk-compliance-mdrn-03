// File: src/modules/iam/CreditRisk.IAM.Application/Ports/ITokenRevocationStore.cs
namespace CreditRisk.IAM.Application.Ports;

public interface ITokenRevocationStore
{
    Task RevokeAsync(string jti, TimeSpan expiresIn, CancellationToken cancellationToken = default);
    Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default);
}
