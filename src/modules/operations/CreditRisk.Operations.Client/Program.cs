// File: src/modules/operations/CreditRisk.Operations.Client/Program.cs
using System.Security.Claims;
using CreditRisk.Operations.Client;
using CreditRisk.Operations.Client.Services;
using CreditRisk.Operations.Client.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// OIDC authentication with Keycloak
builder.Services.AddOidcAuthentication(options =>
{
    var oidcConfig = builder.Configuration.GetSection("Oidc");
    options.ProviderOptions.Authority = oidcConfig["Authority"] ?? "http://localhost:8080/realms/crcl";
    options.ProviderOptions.MetadataUrl = $"{options.ProviderOptions.Authority}/.well-known/openid-configuration";
    options.ProviderOptions.ClientId = oidcConfig["ClientId"] ?? "crcl-blazor-client";
    options.ProviderOptions.ResponseType = oidcConfig["ResponseType"] ?? "code";
    
    options.ProviderOptions.DefaultScopes.Clear();
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("roles");

    options.ProviderOptions.RedirectUri = oidcConfig["RedirectUri"] ?? "http://localhost:5003/authentication/login-callback";
    options.ProviderOptions.PostLogoutRedirectUri = oidcConfig["PostLogoutRedirectUri"] ?? "http://localhost:5003/";

    options.UserOptions.RoleClaim = ClaimTypes.Role;
}).AddAccountClaimsPrincipalFactory<CustomUserFactory>();

// HTTP client
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000/api")
});

// MudBlazor
builder.Services.AddMudServices();

// Application services
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<OperationsHubClient>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<NotificationService>();

await builder.Build().RunAsync();
