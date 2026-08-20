// File: src/shared/CreditRisk.Shared.Kernel/Result/Result.cs
namespace CreditRisk.Shared.Kernel.Result;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail.
/// Use for application-layer methods that can fail for business reasons.
/// Never throw exceptions for expected business failures.
/// </summary>
public sealed class Result<TValue>
{
    private readonly TValue? _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    private Result(TValue value) { IsSuccess = true; _value = value; }
    private Result(Error error) { IsSuccess = false; _error = error; }

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}

/// <summary>Non-generic result for operations that return no value.</summary>
public sealed class Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    private Result() { IsSuccess = true; }
    private Result(Error error) { IsSuccess = false; _error = error; }

    public static Result Success() => new();
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}
