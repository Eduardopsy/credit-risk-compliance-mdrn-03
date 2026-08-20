// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using CreditRisk.IAM.Application.Ports;
using CreditRisk.IAM.Domain.Repositories;
using CreditRisk.IAM.Infrastructure.Persistence.Repositories;
using CreditRisk.IAM.Infrastructure.Redis;
using CreditRisk.IAM.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIamInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
        services.AddScoped<ITokenService, KeycloakTokenService>();

        string redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
        services.AddSingleton<ITokenRevocationStore, RedisTokenRevocationStore>();

        return services;
    }
}
