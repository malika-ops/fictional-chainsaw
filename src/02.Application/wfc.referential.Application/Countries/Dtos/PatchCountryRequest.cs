using System.ComponentModel.DataAnnotations;


namespace wfc.referential.Application.Countries.Dtos;

public record PatchCountryRequest
{
    /// <summary>
    /// The abbreviation of the country.
    /// </summary>
    /// <example>USA</example>
    public string? Abbreviation { get; init; }

    /// <summary>
    /// The full name of the country.
    /// </summary>
    /// <example>United States</example>
    public string? Name { get; init; }

    /// <summary>
    /// Optional code override.
    /// must be unique.
    /// </summary>
    /// <example>US</example>
    public string? Code { get; init; }
    /// <summary>
    /// The ISO2 code of the country.
    /// </summary>
    /// <example>US</example>

    public string? ISO2 { get; init; }
    /// <summary>
    /// The ISO3 code of the country.
    /// </summary>
    /// <example>USA</example>
    public string? ISO3 { get; init; }
    /// <summary>
    /// The international dialing code.
    /// </summary>
    /// <example>+1</example>
    public string? DialingCode { get; init; }
    /// <summary>
    /// The time zone identifier.
    /// </summary>
    /// <example>Eastern Standard Time</example>
    public string? TimeZone { get; init; }

    /// <summary>   
    /// is the country has a sector
    /// </summary>
    public bool? HasSector { get; init; }
    /// <summary>
    /// is the country has a SMS
    /// </summary>
    public bool? IsSmsEnabled { get; init; }
    /// <summary>
    /// the number of decimal digits for the number format
    /// </summary>
    public int? NumberDecimalDigits { get; init; }

    /// <summary>
    /// The ID of the associated MonetaryZone.
    /// Must be a valid (non-empty) GUID.
    /// </summary>
    /// <example>9e2a4b8c-1234-4c2a-9b7b-abcdef123456</example>
    public Guid? MonetaryZoneId { get; init; }

    /// <summary>
    /// Optionally, the Currency ID associated with the country.
    /// </summary>
    /// <example>c1d2e3f4-5678-4c2a-9b7b-fedcba987654</example>
    public Guid? CurrencyId { get; init; }


    /// <summary> filter by country status.</summary>
    public bool? IsEnabled { get; init; }

}
