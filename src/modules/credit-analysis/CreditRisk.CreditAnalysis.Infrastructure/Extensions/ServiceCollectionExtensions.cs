// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using CreditRisk.CreditAnalysis.Application.Ports;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using CreditRisk.CreditAnalysis.Domain.Services;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence.Repositories;
using CreditRisk.CreditAnalysis.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCreditAnalysisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<CreditAnalysisDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CreditAnalysisDb") ?? "Host=localhost;Database=creditanalysis;Username=postgres;Password=postgres"));

        // Repositories
        services.AddScoped<ICreditProposalRepository, CreditProposalRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ICreditScoringEngine, CreditScoringEngine>();

        return services;
    }
}
