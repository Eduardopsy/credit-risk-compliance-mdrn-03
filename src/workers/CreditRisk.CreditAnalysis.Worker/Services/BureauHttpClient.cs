// File: src/workers/CreditRisk.CreditAnalysis.Worker/Services/BureauHttpClient.cs
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CreditRisk.CreditAnalysis.Worker.Services;

/// <summary>
/// HTTP client for querying the bureau mock service.
/// Implements resilience patterns: retry policy and circuit breaker via Polly.
/// </summary>
public sealed class BureauHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BureauHttpClient> _logger;

    public BureauHttpClient(HttpClient httpClient, ILogger<BureauHttpClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Queries bureau for applicant credit data.
    /// Returns null if bureau is unavailable (graceful degradation).
    /// </summary>
    public async Task<BureauQueryResponse?> QueryAsync(string document, string documentType)
    {
        try
        {
            _logger.LogInformation("Querying bureau for document={Document} type={DocumentType}", document, documentType);

            var request = new BureauQueryRequest(document, documentType);
            var response = await _httpClient.PostAsJsonAsync("/query", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bureau query failed with status {StatusCode} for document={Document}",
                    response.StatusCode, document);
                return null; // Graceful degradation: return null on bureau unavailability
            }

            var result = await response.Content.ReadFromJsonAsync<BureauQueryResponse>();
            _logger.LogInformation("Bureau query successful document={Document} score={Score} debt={TotalMonthlyDebt}",
                document, result?.Score, result?.TotalMonthlyDebt);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error querying bureau for document={Document}", document);
            return null; // Graceful degradation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error querying bureau for document={Document}", document);
            return null; // Graceful degradation
        }
    }
}

/// <summary>Request to query credit bureau.</summary>
public sealed record BureauQueryRequest(
    /// <summary>Customer document (CPF/CNPJ).</summary>
    string Document,
    /// <summary>Type of document (CPF, CNPJ, etc.).</summary>
    string DocumentType);

/// <summary>Credit score response from bureau.</summary>
public sealed record BureauQueryResponse(
    /// <summary>Credit score (0-1000).</summary>
    int Score,
    /// <summary>Total monthly debt amount.</summary>
    decimal TotalMonthlyDebt,
    /// <summary>Query status.</summary>
    string Status);
