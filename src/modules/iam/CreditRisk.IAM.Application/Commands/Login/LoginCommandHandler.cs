// File: src/modules/iam/CreditRisk.IAM.Application/Commands/Login/LoginCommandHandler.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Domain.Repositories;
using CreditRisk.IAM.Domain.ValueObjects;
using CreditRisk.IAM.Application.Ports;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(command.Email);
        var user = await userRepository.GetByEmailAsync(emailResult, cancellationToken).ConfigureAwait(false);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash.Value))
        {
            return Result<LoginResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure(Error.Unauthorized("Auth.UserInactive", "User account is inactive."));
        }

        return await tokenService.GenerateTokensAsync(user, cancellationToken).ConfigureAwait(false);
    }
}
