// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Compliance.Domain.Services;
using CreditRisk.Compliance.Infrastructure.Persistence;
using CreditRisk.Compliance.Infrastructure.Persistence.Repositories;
using CreditRisk.Compliance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddComplianceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ComplianceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres") ?? "Host=localhost;Database=creditrisk;Username=crcl;Password=crcl"));

        // Repositories
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAmlAlertRepository, AmlAlertRepository>();

        // Services
        services.AddScoped<IPepScreeningService, PepScreeningService>();
        services.AddScoped<IAmlRulesEngine, AmlRulesEngine>();

        return services;
    }
}
