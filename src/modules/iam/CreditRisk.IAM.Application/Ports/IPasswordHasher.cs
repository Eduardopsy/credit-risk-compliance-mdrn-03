// File: src/modules/iam/CreditRisk.IAM.Application/Ports/IPasswordHasher.cs
namespace CreditRisk.IAM.Application.Ports;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
