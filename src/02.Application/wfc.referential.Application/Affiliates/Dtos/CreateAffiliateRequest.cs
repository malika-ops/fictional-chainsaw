using System.ComponentModel.DataAnnotations;
using wfc.referential.Domain.AffiliateAggregate;

namespace wfc.referential.Application.Affiliates.Dtos;

public record CreateAffiliateRequest
{
    /// <summary>
    /// A unique code identifier for the Affiliate.
    /// </summary>
    /// <example>AFF001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The name of the Affiliate.
    /// </summary>
    /// <example>Wafacash</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The abbreviation of the Affiliate.
    /// </summary>
    /// <example>WFC</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// The opening date of the Affiliate.
    /// </summary>
    /// <example>2023-01-15</example>
    public DateTime OpeningDate { get; init; } 

    /// <summary>
    /// The cancellation day of the Affiliate.
    /// </summary>
    /// <example>Last day of month</example>
    public string CancellationDay { get; init; } = string.Empty;

    /// <summary>
    /// Path or URL to the logo of the Affiliate.
    /// </summary>
    /// <example>/logos/affiliate001.png</example>
    public string Logo { get; init; } = string.Empty;

    /// <summary>
    /// The threshold billing amount for the Affiliate.
    /// </summary>
    /// <example>10000.00</example>
    public decimal ThresholdBilling { get; init; }

    /// <summary>
    /// The accounting document number.
    /// </summary>
    /// <example>ACC-DOC-001</example>
    public string AccountingDocumentNumber { get; init; } = string.Empty;

    /// <summary>
    /// The accounting account number.
    /// </summary>
    /// <example>411000001</example>
    public string AccountingAccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// The stamp duty mention.
    /// </summary>
    /// <example>Stamp duty applicable</example>
    public string StampDutyMention { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the Country.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid CountryId { get; init; }

    /// <summary>
    /// The Affiliate Type parameter type.
    /// </summary>
    /// <example>Paycash</example>
    public AffiliateTypeEnum AffiliateType { get; init; } 
}