using System.ComponentModel;

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
public record GetFiltredCitiesRequest : FilterRequest
{  
    /// <summary>
    /// Optional filter by city code.
    /// </summary>
    /// <example>110</example>
    public string? Code { get; init; }

    /// <summary>
    /// Optional filter by city name.
    /// </summary>
    /// <example>Casablanca</example>
    public string? Name { get; init; }

    /// <summary>
    /// Optional filter by city abbreviation.
    /// </summary>
    /// <example>CSB</example>
    public string? Abbreviation { get; init; }

    /// <summary>
    /// Optional filter by time zone.
    /// </summary>
    /// <example>UTC+1</example>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Optional filter by associated region ID.
    /// </summary>
    /// <example>44449fb6-21a5-47cb-bb2a-bdb28d8b83cf</example>
    public Guid? RegionId { get; init; }
}
