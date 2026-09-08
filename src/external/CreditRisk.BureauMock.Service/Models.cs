// File: src/external/CreditRisk.BureauMock.Service/Models.cs
namespace CreditRisk.BureauMock.Service;

/// <summary>Request to query credit bureau.</summary>
public record QueryRequest(
    /// <summary>Customer document (CPF/CNPJ).</summary>
    string Document,
    /// <summary>Type of document (CPF, CNPJ, etc.).</summary>
    string DocumentType);

/// <summary>Credit score response from bureau.</summary>
public record QueryResponse(
    /// <summary>Credit score (0-1000).</summary>
    int Score,
    /// <summary>Total monthly debt amount.</summary>
    decimal TotalMonthlyDebt,
    /// <summary>Query status.</summary>
    string Status);

/// <summary>Health check response.</summary>
public record BureauHealthResponse(
    string Status,
    DateTimeOffset Timestamp);
