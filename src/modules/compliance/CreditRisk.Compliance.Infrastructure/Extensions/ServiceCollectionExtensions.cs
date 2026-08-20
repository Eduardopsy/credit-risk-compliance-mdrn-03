// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Compliance.Infrastructure.Persistence;
using CreditRisk.Compliance.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddComplianceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAmlAlertRepository, AmlAlertRepository>();
        return services;
    }
}
