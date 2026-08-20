// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using CreditRisk.CreditAnalysis.Application.Ports;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCreditAnalysisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICreditProposalRepository, CreditProposalRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
