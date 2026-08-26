// File: src/modules/iam/CreditRisk.IAM.Application/Commands/Logout/LogoutCommandHandler.cs
using CreditRisk.IAM.Application.Ports;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Commands.Logout;

public sealed class LogoutCommandHandler(ITokenRevocationStore revocationStore)
{
    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        // Validate JTI is not empty
        if (string.IsNullOrWhiteSpace(command.Jti))
        {
            return Result.Failure(Error.Validation("invalid_jti", "JTI cannot be empty"));
        }

        await revocationStore.RevokeAsync(command.Jti, command.ExpiresIn, cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
