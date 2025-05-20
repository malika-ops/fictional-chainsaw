using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Countries.Dtos;

public record UpdateCountryRequest
{

    /// <summary>
    /// The ID of the Country to update.
    /// Must be a valid (non-empty) GUID.
    /// </summary>
    /// <example>e3f1c4b8-8bdf-4c2a-9b7b-776a12345678</example>
    [Required]
    public Guid CountryId { get; init; }

    /// <summary>
    /// The abbreviation of the country.
    /// </summary>
    /// <example>USA</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// The full name of the country.
    /// </summary>
    /// <example>United States</example>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// A short code representing the country.
    /// must be unique.
    /// </summary>
    /// <example>US</example>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The ISO2 code of the country.
    /// </summary>
    /// <example>US</example>
    [Required]
    public string ISO2 { get; init; } = string.Empty;

    /// <summary>
    /// The ISO3 code of the country.
    /// </summary>
    /// <example>USA</example>
    [Required]
    public string ISO3 { get; init; } = string.Empty;

    /// <summary>
    /// The international dialing code.
    /// </summary>
    /// <example>+1</example>
    [Required]
    public string DialingCode { get; init; } = string.Empty;

    /// <summary>
    /// The time zone identifier.
    /// </summary>
    /// <example>Eastern Standard Time</example>
    [Required]
    public string TimeZone { get; init; } = string.Empty;

    /// <summary>   
    /// is the country has a sector
    /// </summary>
    public bool HasSector { get; init; } = false;
    /// <summary>
    /// is the country has a SMS
    /// </summary>
    [Required]
    public bool? IsSmsEnabled { get; init; }
    /// <summary>
    /// the number of decimal digits for the number format
    /// </summary>
    [Required]
    public int NumberDecimalDigits { get; init; }

    /// <summary>
    /// The ID of the associated MonetaryZone.
    /// Must be a valid (non-empty) GUID.
    /// </summary>
    /// <example>9e2a4b8c-1234-4c2a-9b7b-abcdef123456</example>
    [Required]
    public Guid MonetaryZoneId { get; init; }

    /// <summary>
    /// the Currency ID associated with the country.
    /// </summary>
    /// <example>c1d2e3f4-5678-4c2a-9b7b-fedcba987654</example>
    [Required]
    public Guid CurrencyId { get; init; }

    /// <summary> country status.</summary>
    public bool IsEnabled { get; init; } = true;
}
