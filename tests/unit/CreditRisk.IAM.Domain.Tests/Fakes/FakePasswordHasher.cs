// File: tests/unit/CreditRisk.IAM.Domain.Tests/Fakes/FakePasswordHasher.cs
using CreditRisk.IAM.Application.Ports;

namespace CreditRisk.IAM.Domain.Tests.Fakes;

/// <summary>
/// Fake implementation of IPasswordHasher for unit tests.
/// Supports configuring verification results without cryptographic operations.
/// </summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    private bool _verifyResult = true;
    private Exception? _hashThrow;
    private Exception? _verifyThrow;
    private readonly Dictionary<string, string> _hashes = [];

    public string Hash(string password)
    {
        if (_hashThrow is not null)
            throw _hashThrow;

        var hash = $"hashed_{password}_{Guid.NewGuid()}";
        _hashes[password] = hash;
        return hash;
    }

    public bool Verify(string password, string hash)
    {
        if (_verifyThrow is not null)
            throw _verifyThrow;

        return _verifyResult;
    }

    // Test helpers
    public void SetVerifyResult(bool result) => _verifyResult = result;

    public void SetHashThrow(Exception exception) => _hashThrow = exception;

    public void SetVerifyThrow(Exception exception) => _verifyThrow = exception;

    public void Reset()
    {
        _verifyResult = true;
        _hashThrow = null;
        _verifyThrow = null;
        _hashes.Clear();
    }
}
