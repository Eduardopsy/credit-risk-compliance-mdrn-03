// File: src/shared/CreditRisk.Shared.Observability/ObservabilityExtensions.cs
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

namespace CreditRisk.Shared.Observability;

/// <summary>
/// Extension methods to register OpenTelemetry for all Credit Risk services.
/// Call AddCreditRiskObservability in each service's Program.cs.
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing, metrics, and logging for a Credit Risk service.
    /// OTLP export is conditional: if OTEL_EXPORTER_OTLP_ENDPOINT is absent (e.g., when
    /// ASPNETCORE_ENVIRONMENT is not set and appsettings.Development.json is not loaded),
    /// the service starts normally without OTLP — instead of throwing ArgumentNullException.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The service name used in OTel resource attributes.</param>
    public static IServiceCollection AddCreditRiskObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        // ⚠️ CRITICAL: Do NOT use new Uri(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!)
        // If the key is absent (e.g., appsettings.Development.json not loaded because
        // ASPNETCORE_ENVIRONMENT was not exported), new Uri(null) throws ArgumentNullException
        // and the service fails to start entirely. Always null-check first.
        string? otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        string serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health")
                                         && !ctx.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddEntityFrameworkCoreInstrumentation(opts => opts.SetDbStatementForText = true)
                    .AddRedisInstrumentation()
                    .AddHttpClientInstrumentation(opts => { opts.RecordException = true; });

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
            })
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddPrometheusExporter())
            .WithLogging(logging =>
            {
                if (!string.IsNullOrEmpty(otlpEndpoint))
                    logging.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
            });

        return services;
    }
}
