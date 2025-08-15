using System.ComponentModel.DataAnnotations;


namespace wfc.referential.Application.Countries.Dtos;

public record CreateCountryRequest
{
    /// <summary>
    /// The Abbreviation of the country name.
    /// </summary>
    /// <example>USA</example>
    public string? Abbreviation { get; init; } 

    /// <summary>
    /// The country name.
    /// </summary>
    /// <example>United States</example>
    [Required]
    public string Name { get; init; } 

    /// <summary>
    /// A short code for the country.
    /// </summary>
    /// <example>US</example>
    [Required]
    public string Code { get; init; } 

    /// <summary>
    /// The ISO2 code of the country.
    /// </summary>
    /// <example>US</example>
    [Required]
    public string ISO2 { get; init; } 

    /// <summary>
    /// The ISO3 code of the country.
    /// </summary>
    /// <example>USA</example>
    [Required]
    public string ISO3 { get; init; } 

    /// <summary>
    /// The international dialing code.
    /// </summary>
    /// <example>+1</example>
    [Required]
    public string DialingCode { get; init; }

    /// <summary>
    /// The time zone identifier.
    /// </summary>
    /// <example>+5</example>
    [Required]
    public string TimeZone { get; init; }
    /// <summary>   
    /// is the country has a sector
    /// </summary>
    public bool HasSector { get; init; } = false;
    /// <summary>
    /// is the country has a SMS
    /// </summary>
    public bool IsSmsEnabled { get; init; } = false;
    /// <summary>
    /// the number of decimal digits for the number format
    /// </summary>
    public int NumberDecimalDigits { get; init; }

    /// <summary>
    /// The identifier of the associated Monetary Zone.
    /// </summary>
    /// <example>e3f1c4b8-8bdf-4c2a-9b7b-776a12345678</example>
    [Required]
    public Guid MonetaryZoneId { get; init; }

    /// <summary>
    /// Optionally, a currency identifier.
    /// </summary>
    /// <example>e3f1c4b8-8bdf-4c2a-9b7b-776a87654321</example>
    [Required]
    public Guid CurrencyId { get; init; }
}
