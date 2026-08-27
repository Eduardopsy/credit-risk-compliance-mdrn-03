// File: tests/integration/Infrastructure/IntegrationTestFixture.cs
using CreditRisk.CreditAnalysis.Infrastructure.Persistence;
using CreditRisk.Compliance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace CreditRisk.Integration.Tests.Infrastructure;

/// <summary>
/// Integration test fixture that manages Testcontainers for PostgreSQL, RabbitMQ, and Redis.
/// Provides isolated test environment with real services for end-to-end testing.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly RedisContainer _redisContainer;
    private IServiceProvider? _serviceProvider;
    private string _postgresConnectionString = string.Empty;
    private string _rabbitMqConnectionString = string.Empty;
    private string _redisConnectionString = string.Empty;

    public IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("Fixture not initialized");

    public IntegrationTestFixture()
    {
        // Initialize containers (but don't start them yet)
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("creditrisk_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-alpine")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }

    /// <summary>
    /// Starts all containers and initializes services.
    /// Called automatically by xUnit when fixture is used.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start containers in parallel
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _rabbitMqContainer.StartAsync(),
            _redisContainer.StartAsync());

        // Get connection strings from running containers
        _postgresConnectionString = _postgresContainer.GetConnectionString();
        _rabbitMqConnectionString = $"amqp://guest:guest@{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(5672)}";
        _redisConnectionString = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";

        // Build service provider
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger());
        });

        // Register DbContexts
        services.AddDbContext<CreditAnalysisDbContext>(options =>
            options.UseNpgsql(_postgresConnectionString));

        services.AddDbContext<ComplianceDbContext>(options =>
            options.UseNpgsql(_postgresConnectionString));

        // Register infrastructure services
        services.AddCreditAnalysisInfrastructure(new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { KeyValuePair.Create("ConnectionStrings:CreditAnalysisDb", _postgresConnectionString)! })
            .Build());

        services.AddComplianceInfrastructure(new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { KeyValuePair.Create("ConnectionStrings:ComplianceDb", _postgresConnectionString)! })
            .Build());

        _serviceProvider = services.BuildServiceProvider();

        // Run migrations
        await RunMigrationsAsync();
    }

    /// <summary>
    /// Stops all containers and cleans up resources.
    /// Called automatically by xUnit when tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_serviceProvider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();

        await Task.WhenAll(
            _postgresContainer.StopAsync(),
            _rabbitMqContainer.StopAsync(),
            _redisContainer.StopAsync());
    }

    /// <summary>
    /// Gets database context for direct data verification.
    /// </summary>
    public CreditAnalysisDbContext GetCreditAnalysisDbContext()
        => ServiceProvider.GetRequiredService<CreditAnalysisDbContext>();

    /// <summary>
    /// Gets compliance database context.
    /// </summary>
    public ComplianceDbContext GetComplianceDbContext()
        => ServiceProvider.GetRequiredService<ComplianceDbContext>();

    /// <summary>
    /// Gets connection string for manual MassTransit configuration.
    /// </summary>
    public string GetRabbitMqConnectionString() => _rabbitMqConnectionString;

    /// <summary>
    /// Gets Redis connection string for caching tests.
    /// </summary>
    public string GetRedisConnectionString() => _redisConnectionString;

    /// <summary>
    /// Runs EF Core migrations to set up schema.
    /// </summary>
    private async Task RunMigrationsAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        
        var creditAnalysisContext = scope.ServiceProvider.GetRequiredService<CreditAnalysisDbContext>();
        await creditAnalysisContext.Database.MigrateAsync();

        var complianceContext = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
        await complianceContext.Database.MigrateAsync();
    }
}

/// <summary>
/// Collection definition for integration tests to share fixture across test classes.
/// </summary>
[CollectionDefinition("Integration Tests")]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    // This class is intentionally empty, just marks the collection
}
