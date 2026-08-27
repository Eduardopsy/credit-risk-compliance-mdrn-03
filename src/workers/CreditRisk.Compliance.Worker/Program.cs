// File: src/workers/CreditRisk.Compliance.Worker/Program.cs
using CreditRisk.Compliance.Worker.Services;
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

        services.AddComplianceInfrastructure(configuration);

        // Outbox
        services.AddScoped<IOutboxRepository, CreditRisk.Compliance.Infrastructure.Persistence.Repositories.OutboxRepository>();
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

        // HTTP Client for PEP API
        services.AddHttpClient("PepApi", client =>
        {
            var pepSettings = configuration.GetSection("Pep");
            client.BaseAddress = new Uri(pepSettings["ApiBaseUrl"] ?? "http://localhost:8082");
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
