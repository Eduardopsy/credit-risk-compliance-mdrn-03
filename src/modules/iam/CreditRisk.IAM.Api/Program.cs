// File: src/modules/iam/CreditRisk.IAM.Api/Program.cs
using CreditRisk.IAM.Api.Endpoints;
using CreditRisk.IAM.Api.Middleware;
using CreditRisk.IAM.Api.Serialization;
using CreditRisk.IAM.Infrastructure.Persistence;
using CreditRisk.IAM.Application.Commands.Login;
using CreditRisk.IAM.Application.Commands.Logout;
using CreditRisk.IAM.Application.Commands.CreateUser;
using CreditRisk.IAM.Application.Queries.GetUserById;
using CreditRisk.Shared.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using MassTransit;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

// Configure Kestrel to listen on all interfaces
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

builder.Services.AddRouting();
builder.Services.AddCreditRiskObservability(builder.Configuration, serviceName: "iam-api");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, IamApiJsonContext.Default);
});

builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=crcl;SearchPath=iam"));

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

    options.AddPolicy("RequiresComplianceAnalyst", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("roles", "compliance-analyst", "administrator"));

    options.AddPolicy("RequiresAdministrator", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("roles", "administrator"));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Database=creditrisk;Username=crcl;Password=crcl", name: "postgres")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis");

// Add MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost");
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddIamInfrastructure(builder.Configuration);
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<LogoutCommandHandler>();
builder.Services.AddScoped<CreateUserCommandHandler>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<JwtRevocationMiddleware>();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapHealthChecks("/health");

app.MapSwagger();

app.Run();
