// File: src/modules/compliance/CreditRisk.Compliance.Api/Program.cs
using CreditRisk.Compliance.Api.Endpoints;
using CreditRisk.Compliance.Api.Middleware;
using CreditRisk.Compliance.Api.Serialization;
using CreditRisk.Compliance.Infrastructure.Persistence;
using CreditRisk.Compliance.Application.Commands.IngestTransaction;
using CreditRisk.Compliance.Application.Commands.ReviewAlert;
using CreditRisk.Compliance.Application.Queries.ListAlerts;
using CreditRisk.Shared.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

// Configure Kestrel to listen on all interfaces
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002);
});

builder.Services.AddRouting();
builder.Services.AddCreditRiskObservability(builder.Configuration, serviceName: "compliance-api");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ComplianceApiJsonContext.Default);
});

// Database is configured in AddComplianceInfrastructure (below)

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak__Authority"] ?? "http://localhost:8080/realms/credit-risk";
        options.Audience = builder.Configuration["Keycloak__Audience"] ?? "crcl-api";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
        options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequiresComplianceAnalyst", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("roles", "compliance-analyst", "administrator"));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Database=creditrisk;Username=crcl;Password=crcl", name: "postgres");

builder.Services.AddComplianceInfrastructure(builder.Configuration);
builder.Services.AddScoped<IngestTransactionCommandHandler>();
builder.Services.AddScoped<ReviewAlertCommandHandler>();
builder.Services.AddScoped<ListAlertsQueryHandler>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapTransactionEndpoints();
app.MapAlertEndpoints();
app.MapHealthChecks("/health");

app.MapSwagger();

app.Run();
