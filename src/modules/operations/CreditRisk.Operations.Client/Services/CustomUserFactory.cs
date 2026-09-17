// File: src/modules/operations/CreditRisk.Operations.Client/Services/CustomUserFactory.cs
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace CreditRisk.Operations.Client.Services;

/// <summary>
/// Custom ClaimsPrincipalFactory for Keycloak OIDC authentication.
/// Flattens 'roles' array and 'realm_access.roles' into standard ClaimTypes.Role claims.
/// </summary>
public sealed class CustomUserFactory(IAccessTokenProviderAccessor accessor)
    : AccountClaimsPrincipalFactory<RemoteUserAccount>(accessor)
{
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);

        if (account is null)
        {
            return user;
        }

        if (user.Identity is ClaimsIdentity claimsIdentity)
        {
            // 1. Check direct 'roles' property in additional properties
            if (account.AdditionalProperties is not null &&
                account.AdditionalProperties.TryGetValue("roles", out var rolesObj))
            {
                AddRolesFromElement(claimsIdentity, rolesObj);
            }

            // 2. Check 'realm_access.roles'
            if (account.AdditionalProperties is not null &&
                account.AdditionalProperties.TryGetValue("realm_access", out var realmAccessObj) &&
                realmAccessObj is JsonElement realmElement &&
                realmElement.ValueKind == JsonValueKind.Object &&
                realmElement.TryGetProperty("roles", out var realmRoles))
            {
                AddRolesFromElement(claimsIdentity, realmRoles);
            }

            // 3. Fallback: inspect existing claims in claimsIdentity for 'roles' or 'role'
            var roleClaims = claimsIdentity.FindAll("roles").ToList();
            foreach (var claim in roleClaims)
            {
                if (!claimsIdentity.HasClaim(ClaimTypes.Role, claim.Value))
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                }
            }

            // Also check name claim for display
            if (!claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                var preferredUsername = claimsIdentity.FindFirst("preferred_username")?.Value 
                    ?? claimsIdentity.FindFirst("email")?.Value;
                if (!string.IsNullOrEmpty(preferredUsername))
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, preferredUsername));
                }
            }
        }

        return user;
    }

    private static void AddRolesFromElement(ClaimsIdentity identity, object elementObj)
    {
        if (elementObj is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in element.EnumerateArray())
                {
                    var roleStr = role.GetString();
                    if (!string.IsNullOrWhiteSpace(roleStr))
                    {
                        if (!identity.HasClaim(ClaimTypes.Role, roleStr))
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleStr));
                        if (!identity.HasClaim("roles", roleStr))
                            identity.AddClaim(new Claim("roles", roleStr));
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                var roleStr = element.GetString();
                if (!string.IsNullOrWhiteSpace(roleStr))
                {
                    if (!identity.HasClaim(ClaimTypes.Role, roleStr))
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleStr));
                    if (!identity.HasClaim("roles", roleStr))
                        identity.AddClaim(new Claim("roles", roleStr));
                }
            }
        }
    }
}
