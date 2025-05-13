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
public record GetAllRegionsRequest
{
        /// <summary>Page number.</summary>
        /// <example>The default page number is 1</example>
        /// <value>The page number</value>
        public int? PageNumber { get; init; } = 1;

        /// <summary>Page size (default = 10).</summary>
        /// <remarks>The default page size is 10</remarks>
        /// <value>The page size</value>
        public int? PageSize { get; init; } = 10;

        /// <summary>Optional filter by code.</summary>
        public string? Code { get; init; }

        /// <summary>Optional filter by name.</summary>
        public string? Name { get; init; }

        /// <summary>Optional filter by countryId.</summary>
        public Guid? CountryId { get; init; }

        /// <summary>Optional filter by status.</summary>
        public bool? IsEnabled { get; init; } = true;  
}
