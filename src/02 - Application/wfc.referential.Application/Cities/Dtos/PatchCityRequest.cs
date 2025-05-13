using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Dtos;

/// <summary>
/// Represents the data used to partially update a city's information.
/// Only provided fields will be updated; others will remain unchanged.
/// </summary>
/// <example>
/// {
///     "CityId": "6a472a58-5d05-4a1b-8b7f-58516dd614c3",
///     "Code": "NYC",
///     "Name": "New York City",
///     "Abbreviation": "NYC",
///     "TaxZone": "TaxZone1",
///     "TimeZone": "America/New_York",
///     "IsEnabled": true,
///     "RegionId": "e2f6f6de-8bc2-47a2-8e18-123456789abc"
/// }
/// </example>
public record PatchCityRequest
{
    /// <summary>
    /// The unique identifier of the city to update (typically provided as a route parameter).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid CityId { get; init; }

    /// <summary>
    /// Optional new city code. If not provided, the existing code remains unchanged.
    /// </summary>
    /// <example>NYC</example>
    public string? Code { get; init; }

    /// <summary>
    /// Optional new city name. If not provided, the existing name remains unchanged.
    /// </summary>
    /// <example>New York City</example>
    public string? Name { get; init; }

    /// <summary>
    /// Optional new city abbreviation. If not provided, the existing abbreviation remains unchanged.
    /// </summary>
    /// <example>NYC</example>
    public string? Abbreviation { get; init; }

    /// <summary>
    /// Optional new tax zone. If not provided, the existing tax zone remains unchanged.
    /// </summary>
    /// <example>TaxZone1</example>
    public string? TaxZone { get; init; }

    /// <summary>
    /// Optional new time zone. If not provided, the existing time zone remains unchanged.
    /// </summary>
    /// <example>America/New_York</example>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Optional new city status. If not provided, the existing status remains unchanged.
    /// </summary>
    /// <example>Active</example>
    public bool? IsEnabled { get; init; }

    /// <summary>
    /// Optional new region ID. If not provided, the existing region remains unchanged.
    /// </summary>
    /// <example>e2f6f6de-8bc2-47a2-8e18-123456789abc</example>
    public RegionId? RegionId { get; init; }
}
