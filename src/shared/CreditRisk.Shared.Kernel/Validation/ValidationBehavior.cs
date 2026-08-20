// File: src/shared/CreditRisk.Shared.Kernel/Validation/ValidationBehavior.cs
using CreditRisk.Shared.Kernel.Result;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CreditRisk.Shared.Kernel.Validation;

/// <summary>
/// Pipeline behavior that runs FluentValidation before executing a command handler.
/// Returns a validation failure Result without invoking the handler if validation fails.
/// </summary>
/// <typeparam name="TCommand">The command type being validated.</typeparam>
/// <typeparam name="TResult">The result type returned by the handler.</typeparam>
public sealed class ValidationBehavior<TCommand, TResult>(
    IEnumerable<IValidator<TCommand>> validators,
    ILogger<ValidationBehavior<TCommand, TResult>> logger)
    where TCommand : notnull
    where TResult : class
{
    /// <summary>
    /// Validates the command and invokes the next handler if validation passes.
    /// </summary>
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        Func<TCommand, CancellationToken, Task<Result<TResult>>> next,
        CancellationToken cancellationToken = default)
    {
        IValidator<TCommand>[] validatorArray = validators.ToArray();

        if (validatorArray.Length == 0)
            return await next(command, cancellationToken).ConfigureAwait(false);

        var context = new ValidationContext<TCommand>(command);
        var validationResults = await Task.WhenAll(
            validatorArray.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            string commandName = typeof(TCommand).Name;
            logger.LogWarning(
                "Validation failed for {CommandName} with {FailureCount} errors: {Errors}",
                commandName,
                failures.Count,
                string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

            string errorDescription = string.Join("; ", failures.Select(f => f.ErrorMessage));
            return Result<TResult>.Failure(Error.Validation(
                code: $"{commandName}.ValidationFailed",
                description: errorDescription));
        }

        return await next(command, cancellationToken).ConfigureAwait(false);
    }
}
