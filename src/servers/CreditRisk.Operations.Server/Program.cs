// File: src/servers/CreditRisk.Operations.Server/Program.cs
using CreditRisk.Compliance.Infrastructure;
using CreditRisk.Operations.Server.Hubs;
using CreditRisk.Operations.Server.Services;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all interfaces
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003);
});

builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console();
});

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

// Services
builder.Services.AddScoped<AlertNotificationService>();

// Database
builder.Services.AddComplianceInfrastructure(builder.Configuration);

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq");
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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapHub<OperationsHub>("/hub/operations");
app.MapHealthChecks("/health");

app.Run();
