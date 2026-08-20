// File: src/modules/iam/CreditRisk.IAM.Application/Queries/GetUserById/GetUserByIdQueryHandler.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Domain.Repositories;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;

namespace CreditRisk.IAM.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Result<UserDto>.Failure(Error.NotFound("User.NotFound", $"User {query.Id} not found."));
        }

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            FullName = user.FullName,
            Role = user.Role.ToString().ToLowerInvariant(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }
}
