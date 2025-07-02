namespace wfc.referential.Application.Affiliates.Dtos;

public record PatchAffiliateRequest
{
    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>AFF002</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the name. If omitted, name remains unchanged.
    /// </summary>
    /// <example>PayCash</example>
    public string? Name { get; init; }

    /// <summary>
    /// If provided, updates the abbreviation. If omitted, abbreviation remains unchanged.
    /// </summary>
    /// <example>PC</example>
    public string? Abbreviation { get; init; }

    /// <summary>
    /// If provided, updates the opening date. If omitted, opening date remains unchanged.
    /// </summary>
    /// <example>2023-06-01</example>
    public DateTime? OpeningDate { get; init; }

    /// <summary>
    /// If provided, updates the cancellation day. If omitted, cancellation day remains unchanged.
    /// </summary>
    /// <example>15th of month</example>
    public string? CancellationDay { get; init; }

    /// <summary>
    /// If provided, updates the logo. If omitted, logo remains unchanged.
    /// </summary>
    /// <example>/logos/affiliate002.png</example>
    public string? Logo { get; init; }

    /// <summary>
    /// If provided, updates the threshold billing. If omitted, threshold billing remains unchanged.
    /// </summary>
    /// <example>15000.00</example>
    public decimal? ThresholdBilling { get; init; }

    /// <summary>
    /// If provided, updates the accounting document number. If omitted, accounting document number remains unchanged.
    /// </summary>
    /// <example>ACC-DOC-002</example>
    public string? AccountingDocumentNumber { get; init; }

    /// <summary>
    /// If provided, updates the accounting account number. If omitted, accounting account number remains unchanged.
    /// </summary>
    /// <example>411000002</example>
    public string? AccountingAccountNumber { get; init; }

    /// <summary>
    /// If provided, updates the stamp duty mention. If omitted, stamp duty mention remains unchanged.
    /// </summary>
    /// <example>No stamp duty</example>
    public string? StampDutyMention { get; init; }

    /// <summary>
    /// If provided, updates the country. If omitted, country remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? CountryId { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }

    /// <summary>
    /// If provided, updates the Affiliate Type ID. If omitted, Affiliate Type remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa9</example>
    public Guid? AffiliateTypeId { get; init; }
}