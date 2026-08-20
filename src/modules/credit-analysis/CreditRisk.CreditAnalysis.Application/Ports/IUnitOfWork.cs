// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Ports/IUnitOfWork.cs
namespace CreditRisk.CreditAnalysis.Application.Ports;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
