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
public record GetAllProductsRequest
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

        /// <summary>Optional filter by status.</summary>
        public bool? IsEnabled { get; init; } = true;  
}
