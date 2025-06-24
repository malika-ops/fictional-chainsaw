using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.RegionManagement.Dtos;

/// <summary>
/// Represents the request parameters for getting all regions.
/// </summary>
/// <example>
/// {
///     PageNumber = 1,
///     PageSize = 10,
///     Code = "110",
///     Name = "Casablanca-Settat",
///     CountryId = "44449fb6-21a5-47cb-bb2a-bdb28d8b83cf"
/// };
/// </example>
public record GetFiltredRegionsRequest : FilterRequest
{
    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by countryId.</summary>
    public Guid? CountryId { get; init; }
}
