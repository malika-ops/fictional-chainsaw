namespace wfc.referential.Application.Cities.Dtos;

/// <summary>
/// Represents the request to delete a city by its unique identifier.
/// </summary>
/// <example>
/// {
///     "CityId": "6a472a58-5d05-4a1b-8b7f-58516dd614c3"
/// }
/// </example>
public record DeleteCityRequest
{
    /// <summary>
    /// The unique identifier of the city to be deleted.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid CityId { get; init; }
}
