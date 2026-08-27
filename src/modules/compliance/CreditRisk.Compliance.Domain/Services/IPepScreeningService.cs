// File: src/modules/compliance/CreditRisk.Compliance.Domain/Services/IPepScreeningService.cs
namespace CreditRisk.Compliance.Domain.Services;

/// <summary>
/// Service for screening customers against PEP (Politically Exposed Person) databases.
/// Checks if a customer's document (CPF/CNPJ) matches known PEP records.
/// </summary>
public interface IPepScreeningService
{
    /// <summary>
    /// Screens a customer document against the PEP database.
    /// Returns true if the document matches a PEP record (high risk).
    /// </summary>
    /// <param name="document">Customer document (CPF/CNPJ).</param>
    /// <param name="documentType">Type of document (CPF, CNPJ, etc.).</param>
    /// <returns>True if customer is flagged as PEP; false otherwise.</returns>
    Task<bool> IsPersonPoliticallyExposedAsync(string document, string documentType);

    /// <summary>
    /// Gets the risk level of a PEP match (if any).
    /// Higher numbers indicate higher risk.
    /// </summary>
    Task<int> GetPepRiskLevelAsync(string document, string documentType);
}
