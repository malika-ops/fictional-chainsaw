using wfc.referential.Domain.AffiliateAggregate;

namespace wfc.referential.Application.Affiliates.Dtos;

public record GetAffiliatesResponse
{
    /// <summary>
    /// Unique identifier of the affiliate.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid AffiliateId { get; init; }

    /// <summary>
    /// Unique code of the affiliate.
    /// </summary>
    /// <example>AFF001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the affiliate.
    /// </summary>
    /// <example>Acme Corporation</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Abbreviation of the affiliate name.
    /// </summary>
    /// <example>ACME</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// Date when the affiliate was opened.
    /// </summary>
    /// <example>2023-01-15T00:00:00Z</example>
    public DateTime? OpeningDate { get; init; }

    /// <summary>
    /// Day of cancellation, if applicable.
    /// </summary>
    /// <example>2025-12-31</example>
    public string CancellationDay { get; init; } = string.Empty;

    /// <summary>
    /// URL or path to the affiliate's logo.
    /// </summary>
    /// <example>https://example.com/logos/acme.png</example>
    public string Logo { get; init; } = string.Empty;

    /// <summary>
    /// Minimum billing threshold for the affiliate.
    /// </summary>
    /// <example>1000.00</example>
    public decimal ThresholdBilling { get; init; }

    /// <summary>
    /// Accounting document number associated with the affiliate.
    /// </summary>
    /// <example>DOC-2024-001</example>
    public string AccountingDocumentNumber { get; init; } = string.Empty;

    /// <summary>
    /// Accounting account number for the affiliate.
    /// </summary>
    /// <example>401200</example>
    public string AccountingAccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// Stamp duty mention for the affiliate.
    /// </summary>
    /// <example>Exempt from stamp duty</example>
    public string StampDutyMention { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the country where the affiliate is located.
    /// </summary>
    /// <example>f7e6d5c4-b3a2-1098-7654-3210fedcba98</example>
    public Guid CountryId { get; init; }

    /// <summary>
    /// Indicates whether the affiliate is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// The affiliate type, if any.
    /// </summary>
    /// <example>Paycash</example>
    public AffiliateTypeEnum? AffiliateType{ get; init; }
}