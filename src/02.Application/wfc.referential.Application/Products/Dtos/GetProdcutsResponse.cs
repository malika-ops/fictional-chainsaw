using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.Application.Products.Dtos;

public record GetProdcutsResponse
{
    /// <summary>
    /// Unique identifier of the product.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Unique code of the product.
    /// </summary>
    /// <example>PROD001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the product.
    /// </summary>
    /// <example>Money Transfer Product</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the product is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// List of services associated with the product.
    /// </summary>
    public List<GetServicesResponse>? Services { get; init; }
}
