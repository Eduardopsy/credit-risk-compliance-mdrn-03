// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Services/AspNetPasswordHasher.cs
using CreditRisk.IAM.Application.Ports;
using Microsoft.AspNetCore.Identity;

namespace CreditRisk.IAM.Infrastructure.Services;

public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(new object(), hash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
