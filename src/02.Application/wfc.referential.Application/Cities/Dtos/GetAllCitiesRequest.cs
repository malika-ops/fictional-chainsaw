using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Cities.Dtos;
/// <summary>
/// Represents the request parameters for retrieving a paginated list of cities with optional filtering.
/// </summary>
/// <example>
/// {
///     "PageNumber": 1,
///     "PageSize": 10,
///     "Code": "110",
///     "Name": "Casablanca",
///     "Abbreviation": "CSB",
///     "TimeZone": "UTC+1",
///     "RegionId": "44449fb6-21a5-47cb-bb2a-bdb28d8b83cf",
///     "IsEnabled": true
/// }
/// </example>
public record GetAllCitiesRequest
{
    /// <summary>
    /// The page number to retrieve (default is 1).
    /// </summary>
    /// <example>1</example>
    public int? PageNumber { get; set; } = 1;

    /// <summary>
    /// The number of records to retrieve per page (default is 10).
    /// </summary>
    /// <example>10</example>
    public int? PageSize { get; set; } = 10;

    /// <summary>
    /// Optional filter by city code.
    /// </summary>
    /// <example>110</example>
    public string? Code { get; set; }

    /// <summary>
    /// Optional filter by city name.
    /// </summary>
    /// <example>Casablanca</example>
    public string? Name { get; set; }

    /// <summary>
    /// Optional filter by city abbreviation.
    /// </summary>
    /// <example>CSB</example>
    public string? Abbreviation { get; set; }

    /// <summary>
    /// Optional filter by time zone.
    /// </summary>
    /// <example>UTC+1</example>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Optional filter by associated region ID.
    /// </summary>
    /// <example>44449fb6-21a5-47cb-bb2a-bdb28d8b83cf</example>
    public Guid? RegionId { get; set; }

    /// <summary>
    /// Optional filter by city isEnabled (e.g., true, false).
    /// </summary>
    /// <example>true</example>
    public bool? IsEnabled { get; set; }
}
