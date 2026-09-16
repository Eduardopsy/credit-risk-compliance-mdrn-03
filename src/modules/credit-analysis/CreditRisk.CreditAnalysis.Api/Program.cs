// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Program.cs
using CreditRisk.CreditAnalysis.Api.Endpoints;
using CreditRisk.CreditAnalysis.Api.Middleware;
using CreditRisk.CreditAnalysis.Api.Serialization;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence;
using CreditRisk.CreditAnalysis.Application.Commands.CreateProposal;
using CreditRisk.CreditAnalysis.Application.Commands.SubmitProposal;
using CreditRisk.CreditAnalysis.Application.Queries.GetProposalById;
using CreditRisk.Shared.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using MassTransit;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

// Configure Kestrel to listen on all interfaces
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001);
});

builder.Services.AddRouting();
builder.Services.AddCreditRiskObservability(builder.Configuration, serviceName: "credit-analysis-api");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, CreditAnalysisApiJsonContext.Default);
});

builder.Services.AddDbContext<CreditAnalysisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=crcl;SearchPath=credit"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string signingKey = builder.Configuration["Jwt__SecretKey"]
            ?? builder.Configuration["Keycloak__SigningKey"]
            ?? "CreditRiskComplianceLabSecretKeyForJwtSigning2026!";

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequiresDeskOperator", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("roles", "desk-operator", "compliance-analyst", "administrator"));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Database=creditrisk;Username=crcl;Password=crcl", name: "postgres");

// Add MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost");
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCreditAnalysisInfrastructure(builder.Configuration);
builder.Services.AddScoped<CreateProposalCommandHandler>();
builder.Services.AddScoped<SubmitProposalCommandHandler>();
builder.Services.AddScoped<GetProposalByIdQueryHandler>();
builder.Services.AddScoped<CreditRisk.CreditAnalysis.Application.Queries.ListProposals.ListProposalsQueryHandler>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapProposalEndpoints();
app.MapHealthChecks("/health");

app.MapSwagger();

app.Run();
