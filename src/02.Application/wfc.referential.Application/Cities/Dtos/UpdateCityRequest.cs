namespace wfc.referential.Application.Cities.Dtos;

/// <summary>
/// Represents the data required to fully update a city's information.
/// All fields are expected to be provided, even if values remain unchanged.
/// </summary>
/// <example>
/// {
///     "Code": "NYC",
///     "Name": "New York City",
///     "Abbreviation": "NYC",
///     "TimeZone": "America/New_York",
///     "IsEnabled": true,
///     "RegionId": "e2f6f6de-8bc2-47a2-8e18-123456789abc"
/// }
/// </example>
public record UpdateCityRequest
{
    /// <summary>
    /// The updated city code.
    /// </summary>
    /// <example>NYC</example>
    public string? Code { get; init; }

    /// <summary>
    /// The updated city name.
    /// </summary>
    /// <example>New York City</example>
    public string? Name { get; init; }

    /// <summary>
    /// The updated abbreviation for the city.
    /// </summary>
    /// <example>NYC</example>
    public string? Abbreviation { get; init; }

    /// <summary>
    /// The updated time zone of the city (IANA format recommended).
    /// </summary>
    /// <example>America/New_York</example>
    public string? TimeZone { get; init; }

    /// <summary>
    /// The updated IsEnabled of the city (e.g., true, false).
    /// </summary>
    /// <example>Active</example>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// The updated region ID the city is associated with.
    /// </summary>
    /// <example>e2f6f6de-8bc2-47a2-8e18-123456789abc</example>
    public Guid? RegionId { get; init; }
}
