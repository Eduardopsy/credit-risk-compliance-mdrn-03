// File: src/servers/CreditRisk.Operations.Server/Program.cs
using System.Text.Json.Serialization;
using CreditRisk.Compliance.Infrastructure;
using CreditRisk.Operations.Server.Consumers;
using CreditRisk.Operations.Server.Hubs;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console();
});

// Configure Kestrel port (default 5003 for Operations Server)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003);
});

// Authentication with Keycloak JWT Bearer for SignalR & APIs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakAuthority = builder.Configuration["Keycloak:Authority"] 
            ?? builder.Configuration["Keycloak__Authority"] 
            ?? "http://localhost:8080/realms/crcl";

        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = false; // Dev environment
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };

        // Allow SignalR to extract JWT from access_token query param
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/operations"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// SignalR with optional Redis backplane
var signalRBuilder = builder.Services.AddSignalR();
var redisConnectionString = builder.Configuration["SignalR:BackplaneRedis"] 
    ?? builder.Configuration["SignalR__BackplaneRedis"];

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    signalRBuilder.AddStackExchangeRedis(redisConnectionString);
}

builder.Services.AddHealthChecks();

// MassTransit Consumers
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AmlAlertCreatedEventConsumer>();
    x.AddConsumer<CreditLimitApprovedEventConsumer>();
    x.AddConsumer<TransactionFlaggedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] 
            ?? builder.Configuration["RabbitMQ__Host"] 
            ?? "localhost";
        var vhost = builder.Configuration["RabbitMQ:VHost"] 
            ?? builder.Configuration["RabbitMQ__VHost"] 
            ?? "/";
        var username = builder.Configuration["RabbitMQ:Username"] 
            ?? builder.Configuration["RabbitMQ__Username"] 
            ?? "guest";
        var password = builder.Configuration["RabbitMQ:Password"] 
            ?? builder.Configuration["RabbitMQ__Password"] 
            ?? "guest";

        cfg.Host(host, vhost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ReceiveEndpoint("operations-hub_aml-alert-created-event", e =>
        {
            e.ConfigureConsumer<AmlAlertCreatedEventConsumer>(context);
        });

        cfg.ReceiveEndpoint("operations-hub_credit-limit-approved-event", e =>
        {
            e.ConfigureConsumer<CreditLimitApprovedEventConsumer>(context);
        });

        cfg.ReceiveEndpoint("operations-hub_transaction-flagged-event", e =>
        {
            e.ConfigureConsumer<TransactionFlaggedEventConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<OperationsHub>("/hubs/operations");
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

app.Run();
