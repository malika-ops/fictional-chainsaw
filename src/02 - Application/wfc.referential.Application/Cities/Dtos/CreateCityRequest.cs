namespace wfc.referential.Application.Cities.Dtos;

/// <summary>
/// Represents the data required to create a new city.
/// </summary>
/// <example>
/// {
///     "CityCode": "NYC",
///     "CityName": "New York City",
///     "Abbreviation": "NYC",
///     "RegionId": "a3b8b953-2489-49c5-aac4-c97df06d5060",
///     "TimeZone": "America/New_York",
///     "TaxZone": "TaxZone1"
/// }
/// </example>
public record CreateCityRequest
{
    /// <summary>
    /// Unique code representing the city.
    /// </summary>
    /// <example>NYC</example>
    public string CityCode { get; init; } = string.Empty;

    /// <summary>
    /// Full name of the city.
    /// </summary>
    /// <example>New York City</example>
    public string CityName { get; init; } = string.Empty;

    /// <summary>
    /// Optional abbreviation for the city.
    /// </summary>
    /// <example>NYC</example>
    public string? Abbreviation { get; init; }

    /// <summary>
    /// Optional identifier of the region the city belongs to.
    /// </summary>
    /// <example>a3b8b953-2489-49c5-aac4-c97df06d5060</example>
    public Guid? RegionId { get; init; }

    /// <summary>
    /// Time zone of the city (IANA format preferred).
    /// </summary>
    /// <example>America/New_York</example>
    public string TimeZone { get; init; } = string.Empty;

    /// <summary>
    /// Tax zone the city falls under.
    /// </summary>
    /// <example>TaxZone1</example>
    public string TaxZone { get; init; } = string.Empty;
}
