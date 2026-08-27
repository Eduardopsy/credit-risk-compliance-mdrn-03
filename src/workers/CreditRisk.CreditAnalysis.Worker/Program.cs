// File: src/workers/CreditRisk.CreditAnalysis.Worker/Program.cs
using CreditRisk.CreditAnalysis.Worker.Services;
using CreditRisk.Shared.Kernel.Outbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, config) =>
    {
        config
            .MinimumLevel.Information()
            .WriteTo.Console();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddCreditAnalysisInfrastructure(configuration);

        // Outbox
        services.AddScoped<IOutboxRepository, CreditRisk.CreditAnalysis.Infrastructure.Persistence.Repositories.OutboxRepository>();
        services.AddHostedService<OutboxProcessor>();

        // MassTransit
        services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(Program).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqSettings = configuration.GetSection("RabbitMq");
                var hostName = rabbitMqSettings["Host"] ?? "localhost";
                var username = rabbitMqSettings["Username"] ?? "guest";
                var password = rabbitMqSettings["Password"] ?? "guest";

                cfg.Host(hostName, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        // HTTP Client for Bureau API (resilience applied via BureauHttpClient exception handling)
        services.AddHttpClient<BureauHttpClient>(client =>
        {
            var bureauSettings = configuration.GetSection("Bureau");
            client.BaseAddress = new Uri(bureauSettings["ApiBaseUrl"] ?? "http://localhost:8081");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            var redisSettings = configuration.GetSection("Redis");
            var connection = redisSettings["ConnectionString"] ?? "localhost:6379";
            options.Configuration = connection;
        });
    })
    .Build();

await host.RunAsync();
