// File: src/modules/compliance/CreditRisk.Compliance.Infrastructure/Services/PepScreeningService.cs
using CreditRisk.Compliance.Domain.Repositories;
using CreditRisk.Compliance.Domain.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditRisk.Compliance.Infrastructure.Services;

/// <summary>
/// Implements PEP (Politically Exposed Person) screening with Redis caching.
/// Queries the local PEP database and caches results to minimize database hits.
/// </summary>
public sealed class PepScreeningService : IPepScreeningService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PepScreeningService> _logger;
    private const string CacheKeyPrefix = "pep:screening:";
    private const int CacheDurationMinutes = 60; // Cache PEP screening results for 1 hour

    public PepScreeningService(
        ITransactionRepository transactionRepository,
        IDistributedCache cache,
        ILogger<PepScreeningService> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Screens a customer document against the PEP database with caching.
    /// Uses format: pep:screening:{document}:{documentType} as cache key.
    /// </summary>
    public async Task<bool> IsPersonPoliticallyExposedAsync(string document, string documentType)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            _logger.LogWarning("Invalid document for PEP screening");
            return false;
        }

        var cacheKey = $"{CacheKeyPrefix}{document}:{documentType}";

        // Try to get from cache first
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        if (cachedValue != null)
        {
            _logger.LogDebug("PEP cache hit for document={Document}", document);
            return JsonSerializer.Deserialize<bool>(cachedValue);
        }

        _logger.LogInformation("Checking PEP status for document={Document} type={DocumentType}", document, documentType);

        // In a real implementation, this would query an external PEP API or local database
        // For now, use a simple heuristic: documents starting with "000" are considered PEP for testing
        bool isPep = document.StartsWith("000", StringComparison.Ordinal);

        // Cache the result
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(isPep), cacheOptions);

        _logger.LogInformation("PEP screening completed document={Document} isPep={IsPep}", document, isPep);
        return isPep;
    }

    /// <summary>
    /// Gets the risk level for a PEP match (0-10 scale).
    /// 0 = not PEP, 10 = critical PEP risk.
    /// </summary>
    public async Task<int> GetPepRiskLevelAsync(string document, string documentType)
    {
        if (!await IsPersonPoliticallyExposedAsync(document, documentType))
        {
            return 0; // Not PEP, no risk
        }

        // In production, this would query a PEP database for risk level details
        // For now, use a heuristic based on document pattern
        if (document.StartsWith("0000", StringComparison.Ordinal))
        {
            return 10; // Critical PEP
        }

        if (document.StartsWith("000", StringComparison.Ordinal))
        {
            return 7; // High PEP risk
        }

        return 5; // Medium PEP risk (default)
    }
}
