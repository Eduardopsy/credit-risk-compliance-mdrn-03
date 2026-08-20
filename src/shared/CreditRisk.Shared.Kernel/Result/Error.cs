// File: src/shared/CreditRisk.Shared.Kernel/Result/Error.cs
namespace CreditRisk.Shared.Kernel.Result;

/// <summary>Represents a structured error with a code and human-readable description.</summary>
public sealed record Error
{
    public required string Code { get; init; }
    public required string Description { get; init; }
    public int HttpStatusCode { get; init; } = 400;

    public static Error Validation(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 422 };

    public static Error NotFound(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 404 };

    public static Error Unauthorized(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 401 };

    public static Error Conflict(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 409 };

    public static Error BusinessRule(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 422 };
}
