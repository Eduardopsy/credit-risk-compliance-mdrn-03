// File: src/modules/iam/CreditRisk.IAM.Infrastructure/Services/KeycloakTokenService.cs
using CreditRisk.IAM.Application.DTOs;
using CreditRisk.IAM.Application.Ports;
using CreditRisk.IAM.Domain.Entities;
using CreditRisk.Shared.Kernel.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CreditRisk.IAM.Infrastructure.Services;

public sealed class KeycloakTokenService(IConfiguration configuration) : ITokenService
{
    private const string DefaultSigningKey = "CreditRiskComplianceLabSecretKeyForJwtSigning2026!";
    private readonly string _issuer = configuration["Keycloak__Authority"] ?? "http://localhost:8080/realms/credit-risk";
    private readonly string _audience = configuration["Keycloak__Audience"] ?? "crcl-api";
    private readonly string _signingKey = configuration["Jwt__SecretKey"]
        ?? configuration["Keycloak__SigningKey"]
        ?? DefaultSigningKey;

    public Task<Result<LoginResponse>> GenerateTokensAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        string roleName = user.Role switch
        {
            CreditRisk.IAM.Domain.Enums.UserRole.DeskOperator => "desk-operator",
            CreditRisk.IAM.Domain.Enums.UserRole.ComplianceAnalyst => "compliance-analyst",
            CreditRisk.IAM.Domain.Enums.UserRole.Administrator => "administrator",
            _ => user.Role.ToString().ToLowerInvariant()
        };

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email.Value),
            new("name", user.FullName),
            new("roles", roleName),
            new(ClaimTypes.Role, roleName),
            new("jti", Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        string accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = Guid.NewGuid().ToString("N"),
            ExpiresIn = 900,
            TokenType = "Bearer",
            Roles = [roleName]
        };

        return Task.FromResult(Result<LoginResponse>.Success(response));
    }

    public Task<Result<LoginResponse>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new("sub", Guid.NewGuid().ToString()),
            new("roles", "desk-operator"),
            new(ClaimTypes.Role, "desk-operator"),
            new("jti", Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        string accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = Guid.NewGuid().ToString("N"),
            ExpiresIn = 900,
            TokenType = "Bearer",
            Roles = ["desk-operator"]
        };

        return Task.FromResult(Result<LoginResponse>.Success(response));
    }
}
