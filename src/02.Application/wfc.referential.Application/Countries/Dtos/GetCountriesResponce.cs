using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.Application.Countries.Dtos;

public record GetCountriesResponce
{
    /// <summary>
    /// Unique identifier of the country.
    /// </summary>
    /// <example>e3b0c442-98fc-1c14-9afb-4c1a1e1f2a3b</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Abbreviation of the country name.
    /// </summary>
    /// <example>MA</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// Name of the country.
    /// </summary>
    /// <example>Morocco</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Unique code of the country.
    /// </summary>
    /// <example>MA001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// ISO 2-letter code of the country.
    /// </summary>
    /// <example>MA</example>
    public string ISO2 { get; init; } = string.Empty;

    /// <summary>
    /// ISO 3-letter code of the country.
    /// </summary>
    /// <example>MAR</example>
    public string ISO3 { get; init; } = string.Empty;

    /// <summary>
    /// International dialing code of the country.
    /// </summary>
    /// <example>+212</example>
    public string DialingCode { get; init; } = string.Empty;

    /// <summary>
    /// Time zone of the country.
    /// </summary>
    /// <example>Africa/Casablanca</example>
    public string TimeZone { get; init; } = string.Empty;

    /// <summary>
    /// Indicates if the country has sectors.
    /// </summary>
    /// <example>true</example>
    public bool HasSector { get; init; }

    /// <summary>
    /// Indicates if SMS is enabled for the country.
    /// </summary>
    /// <example>true</example>
    public bool IsSmsEnabled { get; init; }

    /// <summary>
    /// Number of decimal digits used in the country.
    /// </summary>
    /// <example>2</example>
    public int NumberDecimalDigits { get; init; }

    /// <summary>
    /// Indicates whether the country is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Currency information for the country.
    /// </summary>
    public GetCurrenciesResponse Currency { get; init; } = default!;

    /// <summary>
    /// Unique identifier of the monetary zone.
    /// </summary>
    /// <example>f7e6d5c4-b3a2-1098-7654-3210fedcba98</example>
    public Guid MonetaryZoneId { get; init; }

    /// <summary>
    /// Date and time when the country was created.
    /// </summary>
    /// <example>2024-01-01T12:00:00+00:00</example>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Date and time when the country was last modified.
    /// </summary>
    /// <example>2024-06-01T12:00:00+00:00</example>
    public DateTimeOffset LastModified { get; init; }

    /// <summary>
    /// User who created the country.
    /// </summary>
    /// <example>admin</example>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// User who last modified the country.
    /// </summary>
    /// <example>editor</example>
    public string LastModifiedBy { get; init; } = string.Empty;
}
