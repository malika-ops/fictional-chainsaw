using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Dtos;

/// <summary>
/// Represents the request parameters for getting all Products.
/// </summary>
/// <example>
/// {
///     PageNumber = 1,
///     PageSize = 10,
///     Code = "001",
///     Name = "Cash Express"
/// };
/// </example>
public record GetFiltredProductsRequest : FilterRequest
{
        /// <summary>Optional filter by code.</summary>
        public string? Code { get; init; }

        /// <summary>Optional filter by name.</summary>
        public string? Name { get; init; }
}
