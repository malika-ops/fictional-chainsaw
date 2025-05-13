using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Dtos;

public record PatchProductRequest
{
    /// <summary>
    /// The string representation of the Product's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid ProductId { get; init; }

    /// <summary> 
    /// If provided, updates the code. If omitted, code remains unchanged. 
    /// </summary>
    public string? Code { get; init; }

    /// <summary> 
    /// If provided, updates the name. If omitted, name remains unchanged. 
    /// </summary>
    public string? Name { get; init; }

    /// <summary> 
    /// If provided, updates the status. If omitted, description remains unchanged. 
    /// </summary>
    public bool? IsEnabled { get; init; }

}
